using OpenDataService.DataSources;
namespace OpenDataService.Web;

public static class HttpContextExtensions
{
    public static void SetDataSource(this HttpContext context, IDataSource dataSource)
    {
        context.Items.TryAdd(typeof(IDataSource), dataSource);
    }
    public static IDataSource GetDataSource(this HttpContext context)
    {
        object? dataSourceObj;
        if (!context.Items.TryGetValue(typeof(IDataSource), out dataSourceObj))
        {
            throw new ArgumentException("Datasource missing");
        }

        var dataSource = dataSourceObj as IDataSource;
        if (dataSource == null)
        {
            throw new ArgumentNullException("Datasource null");
        }
        return dataSource;
    }
}