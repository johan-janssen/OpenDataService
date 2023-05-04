using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using OpenDataService.DataSources;

namespace OpenDataService.Web.OData;

public class DataSourceModelBuilder
{
    public IEdmModel Build(IDataSource dataSource)
    {
        var modelBuilder = new ODataConventionModelBuilder();
        foreach (var entityset in dataSource)
        {
            var func = modelBuilder.GetType().GetMethod(nameof(ODataConventionModelBuilder.EntitySet)).MakeGenericMethod(entityset.ClrType);
            func.Invoke(modelBuilder, new [] {entityset.Name});
        }
        return modelBuilder.GetEdmModel();
    }
}