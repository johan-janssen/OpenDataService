using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;

namespace OpenDataService.DataSources;

public interface IDataSource
{
    Type ClrType { get; }
    IEdmModel Model { get; }
    IQueryable Get();
}