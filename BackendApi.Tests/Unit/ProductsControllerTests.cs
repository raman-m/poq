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

        private async Task<ProductsResponse> AssertActionReturnsProducts(
            Expression<Func<IProductsService, Task<ICollection<Product>>>> setupMethod,
            Func<Task<ProductsResponse>> actor)
        {
            // Arrange
            var products = new List<Product>
            {
                    new Product { Title = "A", Description = "A." },
                    new Product { Title = "B", Description = "B B." },
                    new Product { Title = "C", Description = "C C C." },
            };
            // AllAsync(), FilterAsync()
            productsServiceMock.Setup(setupMethod).ReturnsAsync(products);

            // Act
            ProductsResponse actual = await actor.Invoke();

            // Assert
            Assert.NotNull(actual);

            // AllAsync(), FilterAsync()
            productsServiceMock.Verify(setupMethod);
            Assert.Equal(products.Count, actual.Products.Count());
            Assert.All(products,
                p => Assert.Contains(actual.Products, x => x.Title == p.Title));

            return actual;
        }

        private void ArrangeCalculatingPrices(out int minPrice, out int maxPrice)
        {
            minPrice = DateTime.Now.Second / 10;
            maxPrice = minPrice + DateTime.Now.Second;
            productsServiceMock.Setup(x => x.GetPricingStatistics(It.IsAny<ICollection<Product>>())).Returns((minPrice, maxPrice));
        }

        private void ArrangeGettingAllSizes()
        {
            productsServiceMock.Setup(x => x.GetAllSizes(It.IsAny<ICollection<Product>>()))
                .Returns(new Sizes[] { Sizes.Small, Sizes.Medium });
        }

        private void ArrangeGettingCommonWords(ICollection<string> topWords)
        {
            productsServiceMock.Setup(x => x.GetCommonWordsAsync(It.IsAny<Dictionary<string, int>>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(topWords);
        }

        private void AssertCalculatingPrices(int expectedMinPrice, int expectedMaxPrice, ProductsResponse actual)
        {
            // Assert : Calculates pricing
            productsServiceMock.Verify(x => x.GetPricingStatistics(It.IsAny<ICollection<Product>>()));
            Assert.Equal(expectedMinPrice, actual.MinPrice);
            Assert.Equal(expectedMaxPrice, actual.MaxPrice);
        }

        private void AssertGettingAllSizes()
        {
            productsServiceMock.Verify(x => x.GetAllSizes(It.IsAny<ICollection<Product>>()));
        }

        private void AssertGettingCommonWords(IEnumerable<string> expectedWords, IEnumerable<string> actualWords)
        {
            productsServiceMock.Verify(x => x.GetCommonWordsAsync(It.IsAny<Dictionary<string, int>>(), It.IsAny<int?>(), It.IsAny<int?>()));
            Assert.All(expectedWords,
                eword => Assert.Contains(eword, actualWords));
        }

        [Fact]
        public async Task GetAsync_BeHappy_ReturnsAllProducts()
        {
            await AssertActionReturnsProducts(x => x.AllAsync(),
                () => sut.GetAsync() // Act
            );
        }

        [Fact]
        public async Task GetAsync_BeHappy_CalculatesPricing()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);

            var actual = await AssertActionReturnsProducts(x => x.AllAsync(),
                () => sut.GetAsync() // Act
            );
            AssertCalculatingPrices(minPrice, maxPrice, actual);
        }

        [Fact]
        public async Task GetAsync_BeHappy_GetsAllSizes()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);
            ArrangeGettingAllSizes();

            var actual = await AssertActionReturnsProducts(x => x.AllAsync(),
                () => sut.GetAsync() // Act
            );
            AssertCalculatingPrices(minPrice, maxPrice, actual);
            AssertGettingAllSizes();
        }

        [Fact]
        public async Task GetAsync_BeHappy_GetsCommonWords()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);
            ArrangeGettingAllSizes();
            var expectedWords = new string[] { "C", "B", "A" };
            ArrangeGettingCommonWords(expectedWords);

            var actual = await AssertActionReturnsProducts(x => x.AllAsync(),
                () => sut.GetAsync() // Act
            );
            AssertCalculatingPrices(minPrice, maxPrice, actual);
            AssertGettingAllSizes();
            AssertGettingCommonWords(expectedWords, actual.CommonWords);
        }

        [Fact]
        public async Task FilterAsync_BeHappy_ReturnsAllProducts()
        {
            await AssertActionReturnsProducts(x => x.AllAsync(),
                () => sut.FilterAsync(null, null, null, null) // Act
            );
        }

        [Fact]
        public async Task FilterAsync_BeHappy_ReturnsAllProductsAndCalculatesPricing()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);

            var actual = await AssertActionReturnsProducts(x => x.AllAsync(),
                () => sut.FilterAsync(null, null, null, null) // Act
            );
            AssertCalculatingPrices(minPrice, maxPrice, actual);
        }

        [Fact]
        public async Task FilterAsync_BeHappy_ReturnsAllProductsAndGetsAllSizes()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);
            ArrangeGettingAllSizes();

            var actual = await AssertActionReturnsProducts(x => x.AllAsync(),
                () => sut.FilterAsync(null, null, null, null) // Act
            );

            AssertCalculatingPrices(minPrice, maxPrice, actual);
            AssertGettingAllSizes();
        }

        [Fact]
        public async Task FilterAsync_BeHappy_FiltersProducts()
        {
            await AssertActionReturnsProducts(
                x => x.FilterAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Sizes?>()), // Arrange
                () => sut.FilterAsync(0, 100, null, null) // Act
            );
        }

        [Fact]
        public async Task FilterAsync_BeHappy_CalculatesPricing()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);

            var actual = await AssertActionReturnsProducts(
                x => x.FilterAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Sizes?>()),
                () => sut.FilterAsync(0, 100, null, null)
            );
            AssertCalculatingPrices(minPrice, maxPrice, actual);
        }

        [Fact]
        public async Task FilterAsync_BeHappy_GetsAllSizes()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);
            ArrangeGettingAllSizes();

            var actual = await AssertActionReturnsProducts(
                x => x.FilterAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Sizes?>()),
                () => sut.FilterAsync(0, 100, null, null)
            );

            AssertCalculatingPrices(minPrice, maxPrice, actual);
            AssertGettingAllSizes();
        }

        [Fact]
        public async Task FilterAsync_BeHappy_GetsCommonWords()
        {
            ArrangeCalculatingPrices(out int minPrice, out int maxPrice);
            ArrangeGettingAllSizes();
            var expectedWords = new string[] { "C", "B", "A" };
            ArrangeGettingCommonWords(expectedWords);

            var actual = await AssertActionReturnsProducts(
                x => x.FilterAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Sizes?>()),
                () => sut.FilterAsync(0, 100, null, null)
            );

            AssertCalculatingPrices(minPrice, maxPrice, actual);
            AssertGettingAllSizes();
            AssertGettingCommonWords(expectedWords, actual.CommonWords);
        }
    }
}
