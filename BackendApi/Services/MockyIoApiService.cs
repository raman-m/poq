using Poq.BackendApi.Services.Interfaces;
using Poq.BackendApi.Services.MockyIoApi;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poq.BackendApi.Services;

public class MockyIoApiService : IMockyIoApiService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<MockyIoApiService> logger;

    public MockyIoApiService(
        IHttpClientFactory httpClientFactory,
        ILogger<MockyIoApiService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task<GetResponse?> GetAsync(string? url = null)
    {
        GetResponse? data = null;
        string getUrl = url ?? "http://www.mocky.io/v2/5e307edf3200005d00858b49";

        // No need to dispose, no need to wrap up with "using", so DI container will dispose the HTTP client while current web request disposing
        var httpClient = httpClientFactory.CreateClient(nameof(MockyIoApiService));

        using (var response = await httpClient.GetAsync(getUrl))
        {
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var options = GetJsonOptions();
                    data = await response.Content.ReadFromJsonAsync<GetResponse>(options);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Parsing JSON data of response content has failed. Reason: {Message}.", e.Message);
                }
            }
            else
            {
                logger.LogError("Sending of web request has failed. URL: '{RequestUri}'. Verb: {Method}. Status: {StatusCode}.",
                    response.RequestMessage?.RequestUri, response.RequestMessage?.Method, response.StatusCode);
            }
        }
        return data;
    }

    private JsonSerializerOptions GetJsonOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        options.PropertyNameCaseInsensitive = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        return options;
    }
}
