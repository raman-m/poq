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
            [FromQuery] int? minprice, [FromQuery] int? maxprice,
            [FromQuery] Sizes? size,
            [FromQuery] MultiValueParam? highlight)
        {
            var products = new List<Product>();
            try
            {
                if (!minprice.HasValue && !maxprice.HasValue && !size.HasValue)
                {
                    // Filter is empty, so just return all products
                    return await _repository.SelectAsync();
                }

                products = await _repository.SelectAsync(
                    p =>
                    {
                        if (minprice.HasValue && maxprice.HasValue && size.HasValue)
                            return minprice <= p.Price && p.Price <= maxprice && p.Sizes.Contains(size.Value);

                        if (minprice.HasValue && maxprice.HasValue)
                            return minprice <= p.Price && p.Price <= maxprice;

                        if (minprice.HasValue && size.HasValue)
                            return minprice <= p.Price && p.Sizes.Contains(size.Value);

                        if (maxprice.HasValue && size.HasValue)
                            return p.Price <= maxprice && p.Sizes.Contains(size.Value);

                        if (minprice.HasValue)
                            return minprice <= p.Price;

                        if (maxprice.HasValue)
                            return p.Price <= maxprice;

                        if (size.HasValue)
                            return p.Sizes.Contains(size.Value);

                        return true;
                    });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
            }
            return products;
        }
    }
}
