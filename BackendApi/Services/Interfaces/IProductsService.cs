using Poq.BackendApi.Models;

namespace Poq.BackendApi.Services.Interfaces
{
    public interface IProductsService
    {
        Task<ICollection<Product>> AllAsync();
        Task<ICollection<Product>> FilterAsync(int? minPrice, int? maxPrice, Sizes? size);
        (int, int) GetPricingStatistics(ICollection<Product> products);
        ICollection<Sizes> GetAllSizes(ICollection<Product> products);
        Task<ICollection<string>> GetCommonWordsAsync(IDictionary<string, int>? statistics = null, int ? skip = null, int? take = null);
        void HighlightDescription(ICollection<Product> products, IEnumerable<string> words, string tag = "em");
    }
}
