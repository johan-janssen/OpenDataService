using System.Collections.Generic;
using OpenDataService.DataSources;
using OpenDataService.DataSources.Excel;
using OpenDataService.DataSources.IO;

namespace OpenDataService.Web.Extensions
{
    public class DataSourceProvider : IDataSourceProvider
    {
        public static IDataSourceProvider Create()
        {
            return new DirectoryDataSourceProvider(
            new LocalFilesystemDirectoryWatcher("TestData/"), 
            new [] { 
                new DataSourceFactory("*.xlsx", (Stream stream) => new ExcelDataSource(stream))
                });
        }
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
