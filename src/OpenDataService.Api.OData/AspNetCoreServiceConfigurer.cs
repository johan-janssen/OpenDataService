using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenDataService.Api.OData.Routing;
using Microsoft.Extensions.Options;

namespace OpenDataService.Api.OData;

public class AspNetCoreServiceConfigurer
{
    public AspNetCoreServiceConfigurer()
    {
    }
    public void AddCatchAllDataSourceServices(string routePrefix, IServiceCollection services)
    {
        services.AddMvc().AddApplicationPart(typeof(CatchAllDataSourceController).Assembly);
        services.AddMvc().AddApplicationPart(typeof(CatchAllDataSourceController).Assembly);
        services.AddControllers().AddOData(opt => opt.Count().Filter().EnableQueryFeatures());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, CatchAllDataSourceRoutingProvider>((IServiceProvider provider) => 
            {
                var options = provider.GetRequiredService<IOptions<ODataOptions>>();
                return new CatchAllDataSourceRoutingProvider(options, routePrefix);
            }));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, MyODataRoutingMatcherPolicy>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseODataRouteDebug();
        }
    }
}