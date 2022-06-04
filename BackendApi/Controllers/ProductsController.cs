using Microsoft.AspNetCore.Mvc;
using Poq.BackendApi.Models;
using Poq.BackendApi.Services.Interfaces;
using System.Linq;
using System.Net;

namespace Poq.BackendApi.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _service;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductsService service,
            ILogger<ProductsController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
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
                var products = await _service.AllAsync();
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
            [FromQuery] int? minprice, [FromQuery] int? maxprice,
            [FromQuery] Sizes? size,
            [FromQuery] MultiValueParam? highlight)
        {
            try
            {
                if (!minprice.HasValue && !maxprice.HasValue && !size.HasValue)
                {
                    // Filter is empty, so just return all products
                    return await _service.AllAsync();
                }

                var collection = await _service.FilterAsync(minprice, maxprice, size);
                return collection;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
            }
            return Array.Empty<Product>();
        }
    }
}
