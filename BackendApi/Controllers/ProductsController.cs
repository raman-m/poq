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
        [ProducesResponseType(typeof(ProductsResponse), (int)HttpStatusCode.OK)]
        public async Task<ProductsResponse> GetAsync()
        {
            var model = new ProductsResponse();
            try
            {
                var collection = await _service.AllAsync();
                model.Products = collection;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
            }
            return model;
        }

        // GET: /filter
        [HttpGet]
        [Route("/filter")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ProductsResponse), (int)HttpStatusCode.OK)]
        public async Task<ProductsResponse> FilterAsync(
            [FromQuery] int? minprice, [FromQuery] int? maxprice,
            [FromQuery] Sizes? size,
            [FromQuery] MultiValueParam? highlight)
        {
            var model = new ProductsResponse();
            try
            {
                ICollection<Product> collection = Array.Empty<Product>();
                if (!minprice.HasValue && !maxprice.HasValue && !size.HasValue)
                {
                    // Filter is empty, so just return all products
                    collection = await _service.AllAsync();
                }
                else
                {
                    collection = await _service.FilterAsync(minprice, maxprice, size);
                }
                model.Products = collection;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
            }
            return model;
        }
    }
}
