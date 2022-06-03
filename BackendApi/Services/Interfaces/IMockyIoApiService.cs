using Poq.BackendApi.Services.MockyIoApi;

namespace Poq.BackendApi.Services.Interfaces
{
    public interface IMockyIoApiService
    {
        Task<GetResponse?> GetAsync(string? url = null);
    }
}
