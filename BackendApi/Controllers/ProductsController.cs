using Microsoft.AspNetCore.Mvc;
using Poq.BackendApi.Models;
using Poq.BackendApi.Services.Interfaces;
using System.Net;

namespace Poq.BackendApi.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsRepository _repository;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductsRepository repository,
            ILogger<ProductsController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /products
        [HttpGet(Name = "GetProducts")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<Product>> GetAsync()
        {
            try
            {
                var products = await _repository.SelectAsync();
                return products;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
            }
            return Enumerable.Empty<Product>();
        }

        // GET: /filter
        [HttpGet]
        [Route("/filter")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<Product>> FilterAsync(
            [FromQuery] int? maxprice, 
            [FromQuery] string? size,
            [FromQuery] MultiValueParam? highlight)
        {
            try
            {
                if (maxprice == null && string.IsNullOrEmpty(size) && highlight == null)
                {
                    // Filter is empty, so just return all products
                    return await _repository.SelectAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
            }
            return Enumerable.Empty<Product>();
        }
    }
}
