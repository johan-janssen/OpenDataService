namespace OpenDataService.DataSources;

public interface IDataSourceProvider
{
    public 
    IDictionary<string, IDataSource> DataSources { get; }
}