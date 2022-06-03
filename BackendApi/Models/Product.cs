using System.Collections.Generic;

namespace Poq.BackendApi.Models
{
    public class Product
    {
        public string Title { get; set; }
        public int Price { get; set; }
        public ICollection<Sizes> Sizes { get; set; } = Array.Empty<Sizes>();
        public string Description { get; set; }
    }
}
