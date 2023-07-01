using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenDataService.Web.Extensions;
using OpenDataService.DataSources;
using OpenDataService.Api.OData;

namespace OpenDataService.Web
{
    public class Startup
    {
        private AspNetCoreServiceConfigurer? odataAspNetCoreConfigurer;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IDataSourceProvider, DataSourceProvider>();
            odataAspNetCoreConfigurer = new AspNetCoreServiceConfigurer();
            odataAspNetCoreConfigurer.AddCatchAllDataSourceServices("odata", services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            odataAspNetCoreConfigurer!.Configure(app, env);
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
