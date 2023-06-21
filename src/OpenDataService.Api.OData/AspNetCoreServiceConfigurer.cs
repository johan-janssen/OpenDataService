using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OpenDataService.Api.OData.Routing;
using OpenDataService.DataSources;
using Microsoft.Extensions.Options;

namespace OpenDataService.Api.OData;

public class AspNetCoreServiceConfigurer
{
    private string routePrefix;
    public AspNetCoreServiceConfigurer(string routePrefix)
    {
        this.routePrefix = routePrefix;
    }
    public void ConfigureCatchAllDataSourceServices(IServiceCollection services)
    {
        services.AddMvc().AddApplicationPart(typeof(HandleAllController).Assembly);
        services.AddControllers().AddOData(opt => opt.Count().Filter().EnableQueryFeatures());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingModelInitializer<HandleAllController>>((IServiceProvider provider) => 
            {
                var options = provider.GetRequiredService<IOptions<ODataOptions>>();
                return new ODataRoutingModelInitializer<HandleAllController>(options, routePrefix);
            }));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, MyODataRoutingMatcherPolicy>());
    }
}