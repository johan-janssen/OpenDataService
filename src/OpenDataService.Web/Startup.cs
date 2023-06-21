using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenDataService.Web.Extensions;
using OpenDataService.Api.OData.Routing;
using OpenDataService.DataSources;
using OpenDataService.Web.Controllers;
using Microsoft.Extensions.Options;

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
            services.AddControllers().AddOData(opt => opt.Count().Filter().EnableQueryFeatures());
            services.TryAddSingleton<IDataSourceProvider, DataSourceProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingModelInitializer<HandleAllController>>((IServiceProvider provider) => 
                {
                    var options = provider.GetRequiredService<IOptions<ODataOptions>>();
                    return new ODataRoutingModelInitializer<HandleAllController>(options, "odata");
                }));

            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, MyODataRoutingMatcherPolicy>());
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
