using Microsoft.Extensions.Logging;
using Poq.BackendApi.Controllers;
using Poq.BackendApi.Models;
using Poq.BackendApi.Services.Interfaces;
using System.Linq.Expressions;

namespace Poq.BackendApi.Tests.Unit
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductsService> productsServiceMock;
        private readonly Mock<ILogger<ProductsController>> loggerMock;

        private readonly ProductsController sut;

        public ProductsControllerTests()
        {
            productsServiceMock = new Mock<IProductsService>();
            loggerMock = new Mock<ILogger<ProductsController>>();

            sut = new ProductsController(productsServiceMock.Object, loggerMock.Object);
        }

        private async Task AssertActionReturnsProductsAndCalculatesPricing(
            Expression<Func<IProductsService, Task<ICollection<Product>>>> setupMethod,
            Func<Task<ProductsResponse>> actor)
        {
            // Arrange
            var products = new List<Product>
            {
                    new Product { Title = "A" },
                    new Product { Title = "B" },
                    new Product { Title = "C" },
            };
            // AllAsync(), FilterAsync()
            productsServiceMock.Setup(setupMethod).ReturnsAsync(products);

            int minPrice = 1, maxPrice = 10;
            productsServiceMock.Setup(x => x.GetPricingStatistics(It.IsAny<ICollection<Product>>())).Returns((minPrice, maxPrice));

            // Act
            ProductsResponse actual = await actor.Invoke();

            // Assert
            Assert.NotNull(actual);

            // AllAsync(), FilterAsync()
            productsServiceMock.Verify(setupMethod);
            Assert.Equal(products.Count, actual.Products.Count());
            Assert.All(products,
                p => Assert.Contains(actual.Products, x => x.Title == p.Title));

            // Assert : Calculates pricing
            productsServiceMock.Verify(x => x.GetPricingStatistics(It.IsAny<ICollection<Product>>()));
            Assert.Equal(minPrice, actual.MinPrice);
            Assert.Equal(maxPrice, actual.MaxPrice);
        }

        [Fact]
        public async Task GetAsync_BeHappy_ReturnsAllProductsAndCalculatesPricing()
        {
            await AssertActionReturnsProductsAndCalculatesPricing(
                x => x.AllAsync(),
                () => sut.GetAsync()
            );
        }

        [Fact]
        public async Task FilterAsync_BeHappy_ReturnsAllProductsAndCalculatesPricing()
        {
            await AssertActionReturnsProductsAndCalculatesPricing(
                x => x.AllAsync(),
                () => sut.FilterAsync(null, null, null, null)
            );
        }

        [Fact]
        public async Task FilterAsync_BeHappy_FiltersProductsAndCalculatesPricing()
        {
            await AssertActionReturnsProductsAndCalculatesPricing(
                x => x.FilterAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Sizes?>()),
                () => sut.FilterAsync(0, 100, null, null)
            );
        }
    }
}
