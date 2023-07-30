using System.Collections;

namespace OpenDataService.DataSources.Excel;

public class Sheet
{
    ColumnDefinition[] columns;
    public Sheet(string name, IEnumerable<ColumnDefinition> columns, object[] data)
    {
        Name = name;
        this.columns = columns.ToArray();
        Data = data;
    }

    public string Name { get; }
    public int RowCount { get { return Data.Length / columns.Length; }}
    public IEnumerable<ColumnDefinition> Columns { get { return columns; } }

    public object[] Data { get; }
}