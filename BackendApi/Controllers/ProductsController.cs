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
            [FromQuery] Sizes? size,
            [FromQuery] MultiValueParam? highlight)
        {
            try
            {
                if (!maxprice.HasValue && !size.HasValue && highlight == null)
                {
                    // Filter is empty, so just return all products
                    return await _repository.SelectAsync();
                }
                else if (highlight == null)
                {
                    // No need to highlight. Filter products
                    var filtered = await _repository.SelectAsync(
                        p => (maxprice.HasValue && size.HasValue && p.Price <= maxprice && p.Sizes.Contains(size.Value))
                            || (maxprice.HasValue && !size.HasValue && p.Price <= maxprice)
                            || (!maxprice.HasValue && size.HasValue && p.Sizes.Contains(size.Value))
                    );
                    return filtered;
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
