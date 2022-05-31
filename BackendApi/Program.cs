using Poq.BackendApi.Binders;

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
            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new MultiValueParamModelBinderProvider());
            });
        }
    }
}