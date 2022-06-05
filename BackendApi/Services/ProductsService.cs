using Poq.BackendApi.Models;
using Poq.BackendApi.Services.Interfaces;
using System.Linq;

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

        public ICollection<Sizes> GetAllSizes(ICollection<Product> products)
        {
            var sizes = products
                .SelectMany(x => x.Sizes)
                .Distinct()
                .ToArray();

            return sizes;
        }

        public async Task<ICollection<string>> GetCommonWordsAsync(IDictionary<string, int>? statistics = null, int? skip = null, int? take = null)
        {
            var enumerator = await repository.SelectAsync();
            var products = enumerator.ToList();

            var words = products
                .SelectMany(x => x.Description
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.Trim('!', '?', '.', ',', ':', ';')))
                .ToList();

            var groups = words
                .GroupBy(word => word)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(_ => _.Value)
                .ToList();

            if (statistics != null)
            {
                statistics.Clear();
                foreach (var group in groups)
                {
                    statistics.Add(group.Key, group.Value);
                }
            }

            var common = groups
                .Skip(skip ?? 0)
                .Take(take ?? groups.Count)
                .Select(_ => _.Key)
                .ToList();

            return common;
        }

        public void HighlightDescription(ICollection<Product> products, IEnumerable<string> words, string tag = "em")
        {
            if (products == null || words == null || !words.Any())
                return;

            foreach (var product in products)
            {
                var source = product.Description;

                var highlighted = words.Aggregate(source,
                    (phrase, word) =>
                    {
                        var wrapped = $"<{tag}>{word}</{tag}>";
                        return phrase.Contains(wrapped) ? phrase : phrase.Replace(word, wrapped);
                    });

                product.Description = highlighted;
            }
        }
    }
}
