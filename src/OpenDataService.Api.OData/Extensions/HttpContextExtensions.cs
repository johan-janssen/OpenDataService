using Microsoft.AspNetCore.Http;
using OpenDataService.DataSources;

namespace OpenDataService.Api;

public static class HttpContextExtensions
{
    public static void SetDataSource(this HttpContext context, IDataSource dataSource)
    {
        context.Items.TryAdd(typeof(IDataSource), dataSource);
    }
    public static IDataSource GetDataSource(this HttpContext context)
    {
        if (!context.Items.TryGetValueTyped(typeof(IDataSource), out IDataSource? dataSource))
        {
            throw new ArgumentException("Datasource missing");
        }

        return dataSource!;
    }
}