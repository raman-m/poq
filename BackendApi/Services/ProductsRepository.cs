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

    public async Task Warmup()
    {
        if (warmedUp) return;

        productsTable.Clear();
        productsTableIndex = 0;

        var response = await mockyIoService.GetAsync();

        if (response == null || response.Products == null)
        {
            warmedUp = false;
            logger.LogDebug($"Warming up has failed. Cannot get data from the remote web API.");
            return;
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
            return;
        }

        warmedUp = true;
    }

    public async Task<IEnumerable<Product>> SelectAsync()
    {
        await Warmup();

        Product[] array = productsTable.Values
            .ToArray(); // make new instance

        return array;
    }

    public async Task<IEnumerable<Product>> SelectAsync(Func<Product, bool> predicate)
    {
        await Warmup();

        return productsTable.Values.Where(predicate);
    }

    public async Task<Product> GetAsync(int id)
    {
        await Warmup();

        bool found = productsTable.TryGetValue(id, out Product? product);

        return product;
    }

    public async Task<int> CountAsync()
    {
        await Warmup();

        return productsTable.Count;
    }
}
