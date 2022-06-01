using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Poq.BackendApi.Binders;
using Poq.BackendApi.Models;
using System.Collections;

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

        // Arrange : Value provider returns nothing
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

    [Fact]
    public async Task BindModelAsync_CollectionFormatIsMulti_Success()
    {
        // Arrange
        var service = new MultiValueParamModelBinder();
        string parameterName = "highlight";

        var contextMock = new Mock<ModelBindingContext>();
        contextMock.SetupGet(x => x.ModelName).Returns(parameterName);
        var valueProviderMock = new Mock<IValueProvider>();
        contextMock.SetupGet(x => x.ValueProvider).Returns(valueProviderMock.Object);

        var state = new ModelStateDictionary();
        contextMock.SetupGet(x => x.ModelState).Returns(state);

        ModelBindingResult result = ModelBindingResult.Failed();
        contextMock.SetupSet(x => x.Result = It.IsAny<ModelBindingResult>())
            .Callback<ModelBindingResult>(x =>
            {
                result = x;
                contextMock.SetupGet(x => x.Result).Returns(result);
            });

        // Arrange : Multiple parameters with the same name
        ValueProviderResult values = new ValueProviderResult(new StringValues(new string[] { "1", "22", "333" }));
        valueProviderMock.Setup(x => x.GetValue(parameterName)).Returns(values);

        // Act
        await service.BindModelAsync(contextMock.Object);

        // Assert
        Assert.True(state.Count > 0); // there is a model...
        Assert.True(state.ContainsKey(parameterName)); // ...with exact parameter name...
        contextMock.VerifyGet(x => x.ModelState, Times.Once()); //...and the state was setup once.

        contextMock.VerifySet(x => x.Result = It.IsAny<ModelBindingResult>());
        Assert.True(result.IsModelSet);

        var actualModel = result.Model;
        Assert.NotNull(result.Model);
        Assert.IsAssignableFrom<MultiValueParam>(actualModel);

        MultiValueParam actual = actualModel as MultiValueParam;
        Assert.NotNull(actual);
        Assert.True(actual.Count > 1);
        Assert.Equal(values.Length, actual.Count);
        Assert.All(values, x => actual.Contains(x));
    }

    public static IEnumerable<object[]> SingleParameterValues =>
        new object[][]
        {
            new object[] { "1", 1 },
            new object[] { "word", 1 },
            new object[] { "1#2", 1 },
            new object[] { "1,2,3", 3 },
            new object[] { "1 2 3", 3 },
            new object[] { @"1\2\3", 3 },
            new object[] { "1|2|3", 3 },
            new object[] { "1$2$3", 1 },
            new object[] { "a,bb,wordC,wordD", 4 },
            new object[] { ",1", 1 },
            new object[] { "1,", 1 },
            new object[] { ",1,2,", 2 },
        };

    [Theory]
    [MemberData(nameof(SingleParameterValues))]
    public async Task BindModelAsync_SingleParam_Success(string bindingData, int expextedCount)
    {
        // Arrange
        var service = new MultiValueParamModelBinder();
        string parameterName = "highlight";

        var contextMock = new Mock<ModelBindingContext>();
        contextMock.SetupGet(x => x.ModelName).Returns(parameterName);
        var valueProviderMock = new Mock<IValueProvider>();
        contextMock.SetupGet(x => x.ValueProvider).Returns(valueProviderMock.Object);

        var state = new ModelStateDictionary();
        contextMock.SetupGet(x => x.ModelState).Returns(state);

        ModelBindingResult result = ModelBindingResult.Failed();
        contextMock.SetupSet(x => x.Result = It.IsAny<ModelBindingResult>())
            .Callback<ModelBindingResult>(x =>
            {
                result = x;
                contextMock.SetupGet(x => x.Result).Returns(result);
            });

        // Arrange : Single parameter with with separated values
        ValueProviderResult values = new ValueProviderResult(new StringValues(bindingData));
        valueProviderMock.Setup(x => x.GetValue(parameterName)).Returns(values);

        // Act
        await service.BindModelAsync(contextMock.Object);

        // Assert : The model is set to the state and the state was setup once.
        Assert.True(state.Count > 0);
        Assert.True(state.ContainsKey(parameterName));
        contextMock.VerifyGet(x => x.ModelState, Times.Once());

        // Assert : The result has a model
        contextMock.VerifySet(x => x.Result = It.IsAny<ModelBindingResult>());
        Assert.True(result.IsModelSet);

        // Assert : Valid MVC model type
        var actualModel = result.Model;
        Assert.NotNull(result.Model);
        Assert.IsAssignableFrom<MultiValueParam>(actualModel);

        // Assert : Valid length of produced collection
        MultiValueParam actual = actualModel as MultiValueParam;
        Assert.NotNull(actual);
        Assert.Equal(expextedCount, actual.Count);
    }
}
