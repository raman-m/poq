using Microsoft.AspNetCore.Mvc;
using Poq.BackendApi.Binders;
using Poq.BackendApi.Models;

namespace Poq.BackendApi.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        // GET: /products
        [HttpGet(Name = "GetProducts")]
        public IEnumerable<Product> Get()
        {
            return Enumerable.Range(1, 3).Select(index => new Product
            {
                Name = nameof(Product) + index
            })
            .ToArray();
        }

        // GET: /products/filter
        // GET: /filter
        [HttpGet]
        [Route("filter")]
        [Route("/filter")]
        public IEnumerable<Product> Filter(
            [FromQuery] int? maxprice, 
            [FromQuery] string? size,
            [FromQuery] MultiValueParam? highlight)
        {
            return Enumerable.Empty<Product>();
        }
    }
}
