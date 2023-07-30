using OpenDataService.DataSources;
using OpenDataService.DataSources.Dynamic;

namespace OpenDataService.DataSources.Excel;

public class EntitySet : IEntitySet
{
    private IQueryable queryable;
    public EntitySet(string name, Type type, object[] data)
    {
        ClrType = type;
        Name = name;
        queryable = OpenDataService.DataSources.Dynamic.QueryableBuilder.Build(type, data);
    }
    public string Name { get; }
    public Type ClrType { get; }
    public IQueryable Get()
    {
        return queryable;
    }
}

public class EntitySetBuilder
{
    private string assemblyPrefix;
    public EntitySetBuilder(string assemblyPrefix)
    {
        this.assemblyPrefix = assemblyPrefix;
    }
    public IEnumerable<EntitySet> Build(string datasetName, IEnumerable<Sheet> sheets)
    {
        var sheetsAndColumns = sheets.Select(sheet => new {Sheet = sheet, SheetColumns = InsertIdColumnIfMissing(sheet.Columns)});

        var assemblyDefinition = new AssemblyDefinition(string.Format("{0}.{1}", assemblyPrefix, datasetName),
            sheetsAndColumns.Select(sheet => CreateTypeDefinition(sheet.Sheet.Name, sheet.SheetColumns))
        );
        var types = new TypeGenerator().Generate(assemblyDefinition);
        var sheetsAndTheirGeneratedTypes = sheetsAndColumns.Select(sheet => new {Sheet=sheet.Sheet, SheetColumns=sheet.SheetColumns, Type=types.Values.Single(t => t.Name==sheet.Sheet.Name)});

        return sheetsAndTheirGeneratedTypes.Select(s => Build(s.Sheet, s.SheetColumns, s.Type)).ToArray();
    }

    private ColumnDefinition[] InsertIdColumnIfMissing(IEnumerable<ColumnDefinition> columns)
    {
        if (!columns.Any(col => col.Name == "Id"))
        {
            int autoIncrement = 0;
            columns = columns.Append(new ColumnDefinition(-1, "Id", typeof(int), new object[0], 0, (int rowIndex) => autoIncrement++));
        }
        return columns.ToArray();
    }

    private EntitySet Build(Sheet sheet, IEnumerable<ColumnDefinition> columns, Type t)
    {
        var rows = new List<object>();
        for (int i=0; i<sheet.RowCount; i++)
        {
            var row = Activator.CreateInstance(t)!;
            foreach (var column in columns)
            {
                t.GetProperty(column.Name)?.SetValue(row, column.GetValue(i));
            }
            rows.Add(row);
        }
        return new EntitySet(sheet.Name, t, rows.ToArray());
    }

    private TypeDefinition CreateTypeDefinition(string name, IEnumerable<ColumnDefinition> columns)
    {
        return new TypeDefinition(name,
            columns.Select(column => new PropertyDefinition(column.Name, column.Type))
        );
    }
}