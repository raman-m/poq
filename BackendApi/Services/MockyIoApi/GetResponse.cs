using Poq.BackendApi.Models;

namespace Poq.BackendApi.Services.MockyIoApi
{
    public class GetResponse
    {
        public IEnumerable<Product> Products { get; set; } = Enumerable.Empty<Product>();
        public ApiKeys? ApiKeys { get; set; }

    }
}
