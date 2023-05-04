namespace OpenDataService.DataSources;

public interface IDataSourceProvider
{
    IDictionary<string, IDataSource> DataSources { get; }
}