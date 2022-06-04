using Poq.BackendApi.Models;
using Poq.BackendApi.Services.Interfaces;
using System.Collections.Concurrent;

namespace Poq.BackendApi.Services;

public class ProductsRepository : IProductsRepository
{
    private int productsTableIndex;
    private readonly ConcurrentDictionary<int, Product> productsTable;

    private readonly IMockyIoApiService mockyIoService;
    private readonly ILogger<ProductsRepository> logger;

    public ProductsRepository(
        IMockyIoApiService mockyIoService,
        ILogger<ProductsRepository> logger)
    {
        productsTable = new ConcurrentDictionary<int, Product>();
        productsTableIndex = 0;

        this.mockyIoService = mockyIoService ?? throw new ArgumentNullException(nameof(mockyIoService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private bool warmedUp = false;

    public async Task<bool> WarmupAsync()
    {
        if (warmedUp) return warmedUp;

        productsTable.Clear();
        productsTableIndex = 0;

        var response = await mockyIoService.GetAsync();

        if (response == null || response.Products == null || response.Products.Count() == 0)
        {
            warmedUp = false;
            logger.LogWarning($"Warming up has failed. Cannot get data from the remote web API.");
            return warmedUp;
        }

        bool Ok = true;
        var products = response.Products;
        var productsCount = products.Count();
        foreach (var product in products)
        {
            productsTableIndex++; // emulate index incrementor
            var added = productsTable.TryAdd(productsTableIndex, product);
            Ok = added && Ok;
        }

        if (!Ok || productsCount != productsTable.Count)
        {
            logger.LogDebug("Warming up has failed. Response entities count: {ProductsCount}. Actual number of rows in the table: {RowsCount}",
                productsCount, productsTable.Count);
            return warmedUp;
        }

        warmedUp = true;
        return warmedUp;
    }

    public async Task<IEnumerable<Product>> SelectAsync()
    {
        await WarmupAsync();

        return productsTable.Values;
    }

    public async Task<IEnumerable<Product>> SelectAsync(Func<Product, bool> predicate)
    {
        await WarmupAsync();

        return productsTable.Values.Where(predicate);
    }

    public async Task<Product> GetAsync(int id)
    {
        await WarmupAsync();

        bool found = productsTable.TryGetValue(id, out Product product);

        return product;
    }

    public async Task<int> CountAsync()
    {
        await WarmupAsync();

        return productsTable.Count;
    }
}
