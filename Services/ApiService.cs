using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using RestApiProject.Models;
using RestApiProject.Services.Interfaces;
using RestApiProject.Models.Maps;

public class ApiService : IApiService
{
    private readonly IConfiguration _configuration;

    public ApiService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task UpdateData()
    {
        //Downloading files and updating db
        //await DownloadAndSaveFile("https://rekturacjazadanie.blob.core.windows.net/zadanie/Products.csv", "Products.csv");
        await UpdateProducts();

        //await DownloadAndSaveFile("https://rekturacjazadanie.blob.core.windows.net/zadanie/Inventory.csv", "Inventory.csv");
        await UpdateInventory();

        //await DownloadAndSaveFile("https://rekturacjazadanie.blob.core.windows.net/zadanie/Prices.csv", "Prices.csv");
        await UpdatePrices();
    }

    //Geting single product by sku
    public async Task<ProductDetails> GetProductDetails(string sku)
    {
        try
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                var productDetails = connection.QueryFirstOrDefault<ProductDetails>("SELECT p.Name,p.EAN, p.ProducerName, p.Category, p.DefaultImage, i.Qty, i.Unit, pr.NettPrice, i.ShippingCost FROM Products p JOIN Inventory i ON p.SKU = i.SKU LEFT JOIN Prices pr ON p.SKU = pr.SKU WHERE p.SKU = @SKU;",
                    new
                    {
                        SKU = sku
                    });

                if (productDetails != null)
                {
                    return productDetails;
                }
                else
                {
                    Console.WriteLine("No data for SKU: " + sku);
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while fetching data: " + ex.Message);
            return null;
        }
    }

    private async Task DownloadAndSaveFile(string url, string fileName)
    {
        using (var httpClient = new HttpClient())
        {
            var content = await httpClient.GetStringAsync(url);
            await File.WriteAllTextAsync(fileName, content);
        }
    }

    private async Task UpdateProducts()
    {
        try
        {
            using (var reader = new StreamReader("Products.csv"))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", HasHeaderRecord = true, Quote = '"', ShouldSkipRecord = row => string.IsNullOrWhiteSpace(row.Row.Parser.RawRecord) || row.Row.Parser.Record.Any(field => field == "__empty_line__") }))
            {
                //Mapping product class
                csv.Context.RegisterClassMap<ProductMap>();

                //Getting list of products with specific conditions
                var products = csv.GetRecords<Product>().Where(p =>
                    !p.IsWire &&
                    p.Available &&
                    !p.IsVendor);

                //Connection with db
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    //Loop through each product and insert or update it in the Products table
                    foreach (var product in products)
                    {
                        //Checking if all necessary values are not null
                        if (product.SKU != null && product.Name != null && product.EAN != null && product.ProducerName != null && product.Category != null && product.DefaultImage != null)
                        {
                            //Checking if the product already exists in the database
                            var existingProduct = await connection.QueryFirstOrDefaultAsync<Product>("SELECT SKU FROM Products WHERE SKU = @SKU",
                                new
                                {
                                    SKU = product.SKU
                                });

                            if (existingProduct == null)
                            {
                                //Product doesn't exist - Insert
                                await connection.ExecuteAsync("INSERT INTO Products (SKU, Name, EAN, ProducerName, Category, DefaultImage) VALUES (@SKU, @Name, @EAN, @ProducerName, @Category, @DefaultImage)",
                                    new
                                    {
                                        SKU = product.SKU,
                                        Name = product.Name,
                                        EAN = product.EAN,
                                        ProducerName = product.ProducerName,
                                        Category = product.Category,
                                        DefaultImage = product.DefaultImage
                                    });
                            }
                            else
                            {
                                //Product already exists - Update
                                await connection.ExecuteAsync("UPDATE Products SET Name = @Name, EAN = @EAN, ProducerName = @ProducerName, Category = @Category, DefaultImage = @DefaultImage WHERE SKU = @SKU",
                                    new
                                    {
                                        SKU = product.SKU,
                                        Name = product.Name,
                                        EAN = product.EAN,
                                        ProducerName = product.ProducerName,
                                        Category = product.Category,
                                        DefaultImage = product.DefaultImage
                                    });
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during updating Products: " + ex.Message);
        }
    }

    private async Task UpdateInventory()
    {
        try
        {
            using (var reader = new StreamReader("Inventory.csv"))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," }))
            {
                csv.Context.RegisterClassMap<InventoryMap>();

                //Reading headers
                csv.Read();
                csv.ReadHeader();

                var inventory = csv.GetRecords<Inventory>();

                //Connection with db
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (var inventoryItem in inventory)
                            {
                                //Checking values aren't null
                                if (inventoryItem.SKU == null || inventoryItem.Unit == null || inventoryItem.ShippingCost == null || inventoryItem.Shipping == null || inventoryItem.Qty == null)
                                    continue;

                                //Checking product with specific conditions
                                if (string.Equals(inventoryItem.Shipping, "24h", StringComparison.OrdinalIgnoreCase))
                                {
                                    var existingInventoryItem = connection.QueryFirstOrDefault<Inventory>("SELECT SKU FROM Inventory WHERE SKU = @SKU",
                                        new
                                        {
                                            SKU = inventoryItem.SKU
                                        },
                                        transaction: transaction
                                    );

                                    if (existingInventoryItem == null)
                                    {
                                        //Product doesn't exist - Insert
                                        connection.Execute("INSERT INTO Inventory (SKU, Qty, Unit, ShippingCost) VALUES (@SKU, @Qty, @Unit, @ShippingCost)",
                                            new
                                            {
                                                SKU = inventoryItem.SKU,
                                                Qty = inventoryItem.Qty,
                                                Unit = inventoryItem.Unit,
                                                ShippingCost = inventoryItem.ShippingCost
                                            },
                                            transaction: transaction);
                                    }
                                    else
                                    {
                                        //Product already exist - Update
                                        connection.Execute("UPDATE Inventory SET Qty = @Qty, Unit = @Unit, ShippingCost = @ShippingCost WHERE SKU = @SKU",
                                            new
                                            {
                                                SKU = inventoryItem.SKU,
                                                Qty = inventoryItem.Qty,
                                                Unit = inventoryItem.Unit,
                                                ShippingCost = inventoryItem.ShippingCost
                                            },
                                            transaction: transaction);
                                    }
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while saving to the database: " + ex.Message);
                            transaction.Rollback();
                            Console.WriteLine("Transaction rolled back.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while reading CSV file: " + ex.Message);
        }
    }

    private async Task UpdatePrices()
    {
        try
        {
            using (var reader = new StreamReader("Prices.csv"))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", HasHeaderRecord = false, MissingFieldFound = null, BadDataFound = null }))
            {

                if (csv == null)
                {
                    Console.WriteLine("Error: CsvReader is null.");
                    return;
                }

                //Connection with db
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            while (csv.Read())
                            {
                                //Second column - SKU
                                var sku = csv.GetField<string>(1);

                                //Third column - Nett Price
                                var nettPriceStr = csv.GetField<string>(2);

                                //Checking values aren't null
                                if (sku == null || nettPriceStr == null)
                                    continue;

                                nettPriceStr = nettPriceStr.Replace(",", "").Trim();

                                // Checking if the item already exists in the db
                                var existingItem = await connection.QueryFirstOrDefaultAsync<Price>("SELECT SKU FROM Prices WHERE SKU = @SKU",
                                    new
                                    {
                                        SKU = sku
                                    },
                                    transaction: transaction);

                                if (existingItem == null)
                                {
                                    //Item doesn't exist - Insert
                                    await connection.ExecuteAsync("INSERT INTO Prices (SKU, NettPrice) VALUES (@SKU, @NettPrice)",
                                        new
                                        {
                                            SKU = sku,
                                            NettPrice = nettPriceStr
                                        },
                                        transaction: transaction);
                                }
                                else
                                {
                                    // Item already exists - Update
                                    await connection.ExecuteAsync("UPDATE Prices SET NettPrice = @NettPrice WHERE SKU = @SKU",
                                        new
                                        {
                                            SKU = sku,
                                            NettPrice = nettPriceStr
                                        },
                                        transaction: transaction);
                                }
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error while saving to the database: " + ex.Message);
                            transaction.Rollback();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while reading CSV file: " + ex.Message);
        }
    }
}