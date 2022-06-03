using Microsoft.Extensions.Logging;
using Poq.BackendApi.Controllers;
using Poq.BackendApi.Models;
using Poq.BackendApi.Services.Interfaces;

namespace Poq.BackendApi.Tests.Unit
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductsRepository> productsRepositoryMock;
        private readonly Mock<ILogger<ProductsController>> loggerMock;

        private readonly ProductsController sut;

        public ProductsControllerTests()
        {
            productsRepositoryMock = new Mock<IProductsRepository>();
            loggerMock = new Mock<ILogger<ProductsController>>();

            sut = new ProductsController(productsRepositoryMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task GetAsync_RepositoryIsOnline_ReturnsAllProducts()
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
            IEnumerable<Product> actual = await sut.GetAsync();

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
            productsRepositoryMock.Setup(x => x.SelectAsync()).ReturnsAsync(products);

            // Act
            IEnumerable<Product> actual = await sut.FilterAsync(null, null, null);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(products.Count, actual.Count());
            Assert.All(
                products,
                p => Assert.Contains(actual, x => x.Title == p.Title));
        }


        [Fact]
        public async Task FilterAsync_NoHighlighting_FiltersProducts()
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

            // Act #1
            int maxprice = 2;
            IEnumerable<Product> actual1 = await sut.FilterAsync(maxprice, null, null);

            // Assert #1
            Assert.NotNull(actual1);
            Assert.True(actual1.Count() < products.Count);
            Assert.All(actual1, p => Assert.True(p.Price <= maxprice));
            Assert.Equal(2, actual1.Count());

            // Act #2
            Sizes size = Sizes.Medium;
            IEnumerable<Product> actual2 = await sut.FilterAsync(null, size, null);

            // Assert #1
            Assert.NotNull(actual2);
            Assert.True(actual2.Count() < products.Count);
            Assert.All(actual2, p => Assert.True(p.Sizes.Contains(size)));
            Assert.Equal(2, actual2.Count());

            // Act #3
            IEnumerable<Product> actual3 = await sut.FilterAsync(maxprice, size, null);

            // Assert #1
            Assert.NotNull(actual3);
            Assert.True(actual3.Count() < products.Count);
            Assert.All(actual3, p => Assert.True(p.Sizes.Contains(size)));
            Assert.Single(actual3);
        }
    }
}
