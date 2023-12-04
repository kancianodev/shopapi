using Microsoft.AspNetCore.Mvc;
using RestApiProject.Services.Interfaces;

namespace RestApiProject.Controllers
{
    [ApiController]
    [Route("api")] //Setting route to api
    public class ApiController : ControllerBase
    {
        private readonly IApiService _apiService;

        public ApiController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet("update-data")] //Setting route to update-data
        public async Task<IActionResult> UpdateData()
        {
            try
            {
                await _apiService.UpdateData();
                return Ok("Data updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating data: {ex.Message}");
            }
        }

        [HttpGet("product/{sku}")] //Setting route to product with argument SKU
        public IActionResult GetProduct(string sku)
        {
            try
            {
                var product = _apiService.GetProductDetails(sku);

                if (product == null)
                    return NotFound("Product not found");

                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving product: {ex.Message}");
            }
        }
    }
}
