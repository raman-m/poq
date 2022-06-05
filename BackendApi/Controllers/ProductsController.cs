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
            try
            {
                var collection = await _service.AllAsync();
                return await BuildModel(collection);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
                return ProductsResponse.Empty;
            }
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
                return await BuildModel(collection, highlight);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Controller} : \"{Message}\"", nameof(ProductsController), e.Message);
                return ProductsResponse.Empty;
            }
        }

        private async Task<ProductsResponse> BuildModel(ICollection<Product> collection, MultiValueParam? highlight = null)
        {
            var model = new ProductsResponse();
            model.Products = collection;

            var pricing = _service.GetPricingStatistics(collection);
            model.MinPrice = pricing.Item1;
            model.MaxPrice = pricing.Item2;

            model.Sizes = _service.GetAllSizes(collection);

            // Without statistics, get 10 most common words in the product descriptions, excluding the most common 5.
            model.CommonWords = await _service.GetCommonWordsAsync(null, 5, 10); // so, skip 5 and take 10 elements

            if (highlight != null)
                _service.HighlightDescription(collection, highlight);

            return model;
        }
    }
}
