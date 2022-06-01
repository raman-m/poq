using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Poq.BackendApi.Binders;
using Poq.BackendApi.Models;
using System.Globalization;

namespace Poq.BackendApi.Tests.Integration;

public class MultiValueParamModelBinderIntegrationTests
{
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
