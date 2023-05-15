using System.Collections;

namespace OpenDataService.DataSources.Excel;

public class Sheet
{
    public Sheet(string name, IEnumerable<ColumnDefinition> columns, object[][] data)
    {
        Name = name;
        Columns = columns;
        Data = data;
    }

    public string Name { get; }
    public IEnumerable<ColumnDefinition> Columns { get; }

    public object[][] Data { get; }
}