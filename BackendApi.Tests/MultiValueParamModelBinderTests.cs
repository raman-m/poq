using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Poq.BackendApi.Binders;
using Poq.BackendApi.Models;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Moq;
using Microsoft.AspNetCore.Routing;
using System.Globalization;

namespace Poq.BackendApi.Tests;

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
    public async Task BindModelAsync_NoValueProviderResult_NoResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        services.AddControllers/*AddMvc*/(options =>
        {
            options.ModelBinderProviders.Insert(0, new MultiValueParamModelBinderProvider());
        });

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetService<IOptions<MvcOptions>>();

        //var compositeDetailsProvider = new DefaultCompositeMetadataDetailsProvider(new List<IMetadataDetailsProvider>());
        var compositeDetailsProviderMock = new Mock<ICompositeMetadataDetailsProvider>();

        //var metadataProvider = new DefaultModelMetadataProvider(compositeDetailsProvider);
        var metadataProvider = new DefaultModelMetadataProvider(compositeDetailsProviderMock.Object);

        var modelBinderFactory = new ModelBinderFactory(metadataProvider, options, serviceProvider);

        var validatorMock = new Mock<IObjectModelValidator>();
        var parameterBinder = new ParameterBinder(
            metadataProvider,
            modelBinderFactory,
            validatorMock.Object,
            options,
            NullLoggerFactory.Instance);

        var parameter = new ParameterDescriptor()
        {
            Name = "highlight",
            ParameterType = typeof(MultiValueParam),
            BindingInfo = new BindingInfo { BinderModelName = "highlight", BinderType = typeof(MultiValueParamModelBinder), BindingSource = BindingSource.Query }
        };

        var controllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider // You must set this otherwise BinderTypeModelBinder will not resolve the specified type
            },
            RouteData = new RouteData()
        };

        var modelMetadata = metadataProvider.GetMetadataForType(parameter.ParameterType);

        // Arrange test condition: NoValueProviderResult
        IQueryCollection keyValuePairs = new QueryCollection();
            //new Dictionary<string, StringValues>() {
            //    { parameter.Name, new StringValues("1,2,3") }
            //});
        var valueProvider = new QueryStringValueProvider(BindingSource.Query, keyValuePairs, CultureInfo.CurrentCulture);

        var modelBinder = modelBinderFactory.CreateBinder(new ModelBinderFactoryContext()
        {
            BindingInfo = parameter.BindingInfo,
            Metadata = modelMetadata,
            CacheToken = parameter
        });

        // Act
        var modelBindingResult = await parameterBinder.BindModelAsync(
            controllerContext,
            modelBinder,
            valueProvider,
            parameter,
            modelMetadata,
            value: null);

        // Assert
        Assert.False(modelBindingResult.IsModelSet);
        Assert.Null(modelBindingResult.Model);
    }
}
