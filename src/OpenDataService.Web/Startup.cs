using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenDataService.Web.Extensions;
using OpenDataService.DataSources;
using OpenDataService.Api.OData;

namespace OpenDataService.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IDataSourceProvider, DataSourceProvider>();
            new AspNetCoreServiceConfigurer("odata").ConfigureCatchAllDataSourceServices(services);
            // services.AddControllers().AddOData(opt => opt.Count().Filter().EnableQueryFeatures());
            // services.TryAddSingleton<IDataSourceProvider, DataSourceProvider>();
            // services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingModelInitializer<HandleAllController>>((IServiceProvider provider) => 
            //     {
            //         var options = provider.GetRequiredService<IOptions<ODataOptions>>();
            //         return new ODataRoutingModelInitializer<HandleAllController>(options, "odata");
            //     }));

            // services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, MyODataRoutingMatcherPolicy>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Use odata route debug, /$odata
            app.UseODataRouteDebug();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
