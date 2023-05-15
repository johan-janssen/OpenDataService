namespace OpenDataService.DataSources.Excel;

public class ColumnDefinition
{
    public ColumnDefinition(int index, string name, Type type)
    {
        Index = index;
        Name = name;
        Type = type;
    }

    public int Index { get; }
    public string Name { get; }
    public Type Type { get; }
}
