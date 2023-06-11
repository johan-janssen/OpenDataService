using System.Collections.Generic;
using OpenDataService.DataSources;
using OpenDataService.DataSources.Excel;

namespace OpenDataService.Web.Extensions
{
    public class DataSourceProvider : IDataSourceProvider
    {
        public DataSourceProvider()
        {
            DataSources = new Dictionary<string, IDataSource>
            {
                { "mydatasource", new MyDataSource() },
                { "xls", new ExcelDataSource(File.OpenRead("TestData/excelfile.xlsx")) }
            };
        }

        public IDictionary<string, IDataSource> DataSources { get; }
    }
}
