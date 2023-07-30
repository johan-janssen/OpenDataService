namespace OpenDataService.DataSources.Excel;

public delegate object ValueGetter(int rowIndex);

public class ColumnDefinition
{
    private readonly object[] data;
    private readonly int stride;
    private ValueGetter valueGetter;
    public ColumnDefinition(int index, string name, Type type, object[] data, int stride, ValueGetter? valueGetter=null)
    {
        Index = index;
        Name = name;
        Type = type;
        this.data = data;
        this.stride = stride;
        if (valueGetter != null)
        {
            this.valueGetter = valueGetter;
        }
        else
        {
            this.valueGetter = (rowIndex) => data[rowIndex * stride + index];
        }
    }

    public int Index { get; }
    public string Name { get; }
    public Type Type { get; }

    public object GetValue(int rowIndex)
    {
        return valueGetter(rowIndex);
    }
}
