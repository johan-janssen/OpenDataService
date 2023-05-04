using System.Collections.Generic;
using OpenDataService.DataSources;

namespace OpenDataService.Web.Extensions
{
    public class DataSourceProvider : IDataSourceProvider
    {
        public DataSourceProvider()
        {
            DataSources = new Dictionary<string, IDataSource>
            {
                { "mydatasource", new MyDataSource() }
            };
        }

        public IDictionary<string, IDataSource> DataSources { get; }
    }
}
