namespace Poq.BackendApi.Models
{
    public class ProductsResponse
    {
        public IEnumerable<Product> Products { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public IEnumerable<Sizes> Sizes { get; set; }
    }
}
