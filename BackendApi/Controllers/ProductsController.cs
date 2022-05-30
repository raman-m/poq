using Microsoft.AspNetCore.Mvc;

namespace Poq.BackendApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
    }
}
