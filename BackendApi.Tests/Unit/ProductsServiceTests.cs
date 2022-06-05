using Microsoft.Extensions.Logging;
using Poq.BackendApi.Models;
using Poq.BackendApi.Services;
using Poq.BackendApi.Services.Interfaces;

namespace Poq.BackendApi.Tests.Unit;
public class ProductsServiceTests
{
    private readonly Mock<IProductsRepository> productsRepositoryMock;
    private readonly Mock<ILogger<ProductsService>> loggerMock;

    private readonly ProductsService sut;

    public ProductsServiceTests()
    {
        productsRepositoryMock = new Mock<IProductsRepository>();
        loggerMock = new Mock<ILogger<ProductsService>>();

        sut = new ProductsService(productsRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task AllAsync_RepositoryIsOnline_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
            {
                    new Product { Title = "A" },
                    new Product { Title = "B" },
                    new Product { Title = "C" },
            };
        productsRepositoryMock.Setup(x => x.SelectAsync()).ReturnsAsync(products);

        // Act
        ICollection<Product> actual = await sut.AllAsync();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(products.Count, actual.Count());
        Assert.All(
            products,
            p => Assert.Contains(actual, x => x.Title == p.Title));
    }

    [Fact]
    public async Task FilterAsync_NoParams_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
            {
                    new Product { Title = "A" },
                    new Product { Title = "B" },
                    new Product { Title = "C" },
            };
        productsRepositoryMock.Setup(x => x.SelectAsync(It.IsAny<Func<Product, bool>>())).ReturnsAsync(products);

        // Act
        ICollection<Product> actual = await sut.FilterAsync(null, null, null);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(products.Count, actual.Count);
        Assert.All(
            products,
            p => Assert.Contains(actual, x => x.Title == p.Title));
    }


    [Fact]
    public async Task FilterAsync_SomeParamsHaveValues_FiltersProducts()
    {
        // Arrange
        var products = new List<Product>
            {
                    new Product { Title = "A", Price = 1, Sizes = new Sizes[] { Sizes.Small } },
                    new Product { Title = "B", Price = 2, Sizes = new Sizes[] { Sizes.Small, Sizes.Medium } },
                    new Product { Title = "C", Price = 3, Sizes = new Sizes[] { Sizes.Small, Sizes.Medium, Sizes.Large } },
            };

        Func<Product, bool> predicate = p => true;

        productsRepositoryMock.Setup(x => x.SelectAsync(It.IsAny<Func<Product, bool>>()))
            .Callback<Func<Product, bool>>(x => predicate = x)
            .ReturnsAsync(() => products.Where(predicate).ToList());

        int minprice = 0, maxprice = 3;

        // Act #1
        minprice = 3;
        ICollection<Product> actual1 = await sut.FilterAsync(minprice, null, null);

        // Assert #1
        Assert.NotNull(actual1);
        Assert.True(actual1.Count < products.Count);
        Assert.All(actual1, p => Assert.True(p.Price >= minprice));
        Assert.Single(actual1);
        Assert.Contains(actual1, p => p.Title == "C");

        // Act #2
        maxprice = 2;
        ICollection<Product> actual2 = await sut.FilterAsync(null, maxprice, null);

        // Assert #2
        Assert.NotNull(actual2);
        Assert.True(actual2.Count < products.Count);
        Assert.All(actual2, p => Assert.True(p.Price <= maxprice));
        Assert.Equal(2, actual2.Count());

        // Act #3
        minprice = 2;
        maxprice = 3;
        ICollection<Product> actual3 = await sut.FilterAsync(minprice, maxprice, null);

        // Assert #3
        Assert.NotNull(actual3);
        Assert.True(actual3.Count < products.Count);
        Assert.All(actual3, p => Assert.True(p.Price >= minprice));
        Assert.All(actual3, p => Assert.True(p.Price <= maxprice));
        Assert.Equal(2, actual3.Count);
        Assert.Contains(actual3, p => p.Title == "B");
        Assert.Contains(actual3, p => p.Title == "C");

        // Act #4
        Sizes size = Sizes.Medium;
        ICollection<Product> actual4 = await sut.FilterAsync(null, null, size);

        // Assert #4
        Assert.NotNull(actual4);
        Assert.True(actual4.Count < products.Count);
        Assert.All(actual4, p => Assert.True(p.Sizes.Contains(size)));
        Assert.Equal(2, actual4.Count);

        // Act #5
        maxprice = 3;
        size = Sizes.Large;
        ICollection<Product> actual5 = await sut.FilterAsync(null, maxprice, size);

        // Assert #5
        Assert.NotNull(actual5);
        Assert.True(actual5.Count < products.Count);
        Assert.All(actual5, p => Assert.True(p.Price <= maxprice));
        Assert.All(actual5, p => Assert.True(p.Sizes.Contains(size)));
        Assert.Single(actual5);
    }

    [Fact]
    public async Task GetCommonWordsAsync_NoPagination_GetsAllWordsWithStatistics()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Title= "A Blue Socks", Description = "aa aaa aa aaa, aaa abc. " },
            new Product { Title= "A Blue Hat", Description = "This hat perfectly pairs with a red tie." },
        };
        productsRepositoryMock.Setup(x => x.SelectAsync()).ReturnsAsync(products);

        // Act
        var stats = new Dictionary<string, int>();
        ICollection<string> actual = await sut.GetCommonWordsAsync(stats);

        // Assert
        Assert.NotEmpty(actual);

        Assert.True(actual.Contains("a"));
        Assert.True(stats.ContainsKey("a"));
        Assert.Equal(1, stats["a"]);

        Assert.True(actual.Contains("aaa"));
        Assert.True(stats.ContainsKey("aaa"));
        Assert.Equal(3, stats["aaa"]);

        Assert.Equal("aaa", actual.First());
        Assert.Equal(stats.Count, actual.Count);
    }

    [Fact]
    public async Task GetCommonWordsAsync_WithPagination_GetsTopWords()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Description = "1. 2 2. 3 3 3. 4 4 4 4. 5 5 5 5 5." },
        };
        productsRepositoryMock.Setup(x => x.SelectAsync()).ReturnsAsync(products);

        int skip = 1, take = 3;

        // Act
        var stats = new Dictionary<string, int>();
        ICollection<string> actual = await sut.GetCommonWordsAsync(stats, skip, take);

        // Assert
        Assert.NotEmpty(actual);
        Assert.True(actual.Count < stats.Count);
        Assert.Equal(take, actual.Count);
        Assert.Equal("4", actual.First()); // "4" is in top(1)
        Assert.Equal("4", stats.Skip(skip).First().Key); // "4" is in top(1)
    }
}
