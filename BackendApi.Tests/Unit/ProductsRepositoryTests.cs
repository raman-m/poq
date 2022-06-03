using Microsoft.Extensions.Logging;
using Poq.BackendApi.Models;
using Poq.BackendApi.Services;
using Poq.BackendApi.Services.Interfaces;
using Poq.BackendApi.Services.MockyIoApi;

namespace Poq.BackendApi.Tests.Unit
{
    public class ProductsRepositoryTests
    {
        [Fact]
        public async Task WarmupAsync_ColdStartAndNoResponse_NotWarmedUp()
        {
            // Arrange
            var mockyIoServiceMock = new Mock<IMockyIoApiService>();
            var loggerMock = new Mock<ILogger<ProductsRepository>>();

            // Arrange : No response
            mockyIoServiceMock.Setup(x => x.GetAsync(It.IsAny<string?>())).ReturnsAsync(new GetResponse());

            // Arrange : Cold start
            var sut = new ProductsRepository(mockyIoServiceMock.Object, loggerMock.Object);

            // Act
            var actualWarmedUp = await sut.WarmupAsync();

            // Assert
            Assert.False(actualWarmedUp);
        }

        private GetResponse ValidResponse()
        {
            return new GetResponse
            {
                ApiKeys = new ApiKeys { Primary = "A", Secondary = "B" },
                Products = new List<Product>
                {
                    new Product { Title = "A" },
                    new Product { Title = "B" },
                    new Product { Title = "C" },
                }
            };
        }

        [Fact]
        public async Task WarmupAsync_ColdStartAndValidResponse_WarmedUpSuccessfully()
        {
            // Arrange
            var mockyIoServiceMock = new Mock<IMockyIoApiService>();
            var loggerMock = new Mock<ILogger<ProductsRepository>>();

            // Arrange : Valid response
            var response = ValidResponse();
            mockyIoServiceMock.Setup(x => x.GetAsync(It.IsAny<string?>())).ReturnsAsync(response);

            // Arrange : Cold start but with OK response
            var sut = new ProductsRepository(mockyIoServiceMock.Object, loggerMock.Object);

            // Act
            var actualWarmedUp = await sut.WarmupAsync();

            // Assert
            Assert.True(actualWarmedUp);
        }

        [Fact]
        public async Task SelectAsync_ValidResponse_GetsAllProducts()
        {
            // Arrange
            var mockyIoServiceMock = new Mock<IMockyIoApiService>();
            var loggerMock = new Mock<ILogger<ProductsRepository>>();

            // Arrange : Valid response
            var response = ValidResponse();
            mockyIoServiceMock.Setup(x => x.GetAsync(It.IsAny<string?>())).ReturnsAsync(response);

            // Arrange : Cold start but with OK response
            var sut = new ProductsRepository(mockyIoServiceMock.Object, loggerMock.Object);

            // Act
            var actualProducts = await sut.SelectAsync();

            // Assert
            Assert.NotNull(actualProducts);
            Assert.Equal(response.Products.Count(), actualProducts.Count());
        }
    }
}
