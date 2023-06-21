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
        var assemblyDefinition = new AssemblyDefinition(string.Format("{0}.{1}", assemblyPrefix, datasetName),
            sheets.Select(sheet => CreateTypeDefinition(sheet.Name, sheet.Columns))
        );
        var types = new TypeGenerator().Generate(assemblyDefinition);
        var sheetsAndTheirGeneratedTypes = sheets.Select(sheet => new {Sheet=sheet, Type=types.Values.Single(t => t.Name==sheet.Name)});

        return sheetsAndTheirGeneratedTypes.Select(s => Build(s.Sheet, s.Type)).ToArray();
    }

    private EntitySet Build(Sheet sheet, Type t)
    {
        var rows = new List<object>();
        var data = sheet.Data;
        for (int i=0; i<data.Length; i++)
        {
            var row = Activator.CreateInstance(t)!;
            foreach (var column in sheet.Columns)
            {
                t.GetProperty(column.Name)?.SetValue(row, data[i][column.Index]);
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