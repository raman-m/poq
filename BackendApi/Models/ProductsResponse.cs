namespace Poq.BackendApi.Models
{
    public class ProductsResponse
    {
        public static ProductsResponse Empty => new ProductsResponse();

        public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public IEnumerable<Sizes> Sizes { get; set; } = Enumerable.Empty<Sizes>();
        public IEnumerable<string> CommonWords { get; set; } = Enumerable.Empty<string>();
    }
}
