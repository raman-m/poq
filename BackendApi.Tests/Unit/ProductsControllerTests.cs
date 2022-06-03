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
    }
}
