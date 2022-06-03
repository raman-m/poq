using Poq.BackendApi.Binders;
using Poq.BackendApi.Services;
using Poq.BackendApi.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poq.BackendApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Add services to the DI-container.

            var services = builder.Services;

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new MultiValueParamModelBinderProvider());
            })
            .AddJsonOptions(options =>
            {
                options.AllowInputFormatterExceptionMessages = true;

                var jOptions = options.JsonSerializerOptions;
                jOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
                jOptions.PropertyNameCaseInsensitive = true;
                jOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // Add application services
            ConfigureServices(services);

            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IProductsRepository, ProductsRepository>(); // has cross request state
            services.AddTransient<IMockyIoApiService, MockyIoApiService>(); // web API integration, so instantiation per each call
            services.AddHttpClient(nameof(MockyIoApiService)); // to keep performance of HTTP Client high
        }
    }
}