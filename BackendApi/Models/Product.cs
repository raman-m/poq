using System.Collections.Generic;

namespace Poq.BackendApi.Models
{
    public class Product
    {
        public string Title { get; set; }
        public int Price { get; set; }
        public ICollection<Sizes> Sizes { get; set; }
        public string Description { get; set; }

        public Product()
        {
            Title = string.Empty;
            Price = 0;
            Sizes = Array.Empty<Sizes>();
            Description = string.Empty;
        }

        public Product(Product from)
        {
            Title = from.Title;
            Price = from.Price;
            Sizes = (ICollection<Sizes>)(from.Sizes.ToArray().Clone());
            Description = string.IsNullOrEmpty(from.Description)
                ? string.Empty
                : (string)from.Description.Clone();
        }
    }
}
