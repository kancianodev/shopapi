using System.Threading.Tasks;
using RestApiProject.Models;

namespace RestApiProject.Services.Interfaces
{
    public interface IApiService
    {
        Task UpdateData();
        Task<ProductDetails> GetProductDetails(string sku);
    }
}
