using Microsoft.AspNetCore.Mvc.ModelBinding;
using Poq.BackendApi.Binders;

namespace Poq.BackendApi.Tests.Unit;

public class MultiValueParamModelBinderTests
{
    [Fact]
    public void DetectSeparator_NoSource_Failed()
    {
        // Arrange, Act
        var isDetected = MultiValueParamModelBinder.DetectSeparator(string.Empty, out char? separator);

        // Assert
        Assert.Null(separator);
        Assert.False(isDetected);
    }

    [Fact]
    public void DetectSeparator_UknownSeparator_Failed()
    {
        // Arrange, Act
        var isDetected = MultiValueParamModelBinder.DetectSeparator("1#2#3", out char? separator);

        // Assert
        Assert.Null(separator);
        Assert.False(isDetected);
    }

    [Fact]
    public void DetectSeparator_ValidSource_Success()
    {
        // Arrange, Act
        var isDetected = MultiValueParamModelBinder.DetectSeparator("1,2,3", out char? separator);

        // Assert
        Assert.NotNull(separator);
        Assert.True(isDetected);
        Assert.Equal(',', separator.Value);
    }

    [Fact]
    public async Task BindModelAsync_NoBindingContext_ThrowsException()
    {
        // Arrange
        var service = new MultiValueParamModelBinder();
        ModelBindingContext noContext = null;

        // Act, Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.BindModelAsync(noContext));

        Assert.NotNull(exception);
        Assert.Equal("bindingContext", exception.ParamName);
    }

    [Fact]
    public async Task BindModelAsync_NoValueProviderResult_ModelStateWasNotSet()
    {
        // Arrange
        var service = new MultiValueParamModelBinder();
        string parameterName = "highlight";

        var contextMock = new Mock<ModelBindingContext>();
        contextMock.SetupGet(x => x.ModelName).Returns(parameterName);
        var valueProviderMock = new Mock<IValueProvider>();
        contextMock.SetupGet(x => x.ValueProvider).Returns(valueProviderMock.Object);

        valueProviderMock.Setup(x => x.GetValue(parameterName)).Returns(ValueProviderResult.None);

        var state = new ModelStateDictionary();
        contextMock.SetupGet(x => x.ModelState).Returns(state);

        // Act
        await service.BindModelAsync(contextMock.Object);

        // Assert
        Assert.True(state.Count == 0); // model was not set
        Assert.False(state.ContainsKey(parameterName));
        contextMock.VerifyGet(x => x.ModelState, Times.Never());
    }
}
