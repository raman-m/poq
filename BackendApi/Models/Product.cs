namespace Poq.BackendApi.Models
{
    public class Product
    {
        public string Title { get; set; }
        public int Price { get; set; }
        public IEnumerable<Sizes> Sizes { get; set; } = Enumerable.Empty<Sizes>();
        public string Description { get; set; }
    }
}
