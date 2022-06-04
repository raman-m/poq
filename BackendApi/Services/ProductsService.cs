using Poq.BackendApi.Models;
using Poq.BackendApi.Services.Interfaces;

namespace Poq.BackendApi.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IProductsRepository repository;
        private readonly ILogger<ProductsService> logger;

        public ProductsService(
            IProductsRepository repository,
            ILogger<ProductsService> logger)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ICollection<Product>> AllAsync()
        {
            var enumerator = await repository.SelectAsync();
            return enumerator.ToList();
        }

        public async Task<ICollection<Product>> FilterAsync(int? minPrice, int? maxPrice, Sizes? size)
        {
            var enumerator = await repository.SelectAsync(
                p =>
                {
                    if (minPrice.HasValue && maxPrice.HasValue && size.HasValue)
                        return minPrice <= p.Price && p.Price <= maxPrice && p.Sizes.Contains(size.Value);

                    if (minPrice.HasValue && maxPrice.HasValue)
                        return minPrice <= p.Price && p.Price <= maxPrice;

                    if (minPrice.HasValue && size.HasValue)
                        return minPrice <= p.Price && p.Sizes.Contains(size.Value);

                    if (maxPrice.HasValue && size.HasValue)
                        return p.Price <= maxPrice && p.Sizes.Contains(size.Value);

                    if (minPrice.HasValue)
                        return minPrice <= p.Price;

                    if (maxPrice.HasValue)
                        return p.Price <= maxPrice;

                    if (size.HasValue)
                        return p.Sizes.Contains(size.Value);

                    return true;
                });

            return enumerator.ToList();
        }

        public (int, int) GetPricingStatistics(ICollection<Product> products)
        {
            int min = 0, max = 0;
            if (products == null || products.Count == 0)
                return (min, max);

            min = products.Min(x => x.Price);
            max = products.Max(x => x.Price);

            return (min, max);
        }
    }
}
