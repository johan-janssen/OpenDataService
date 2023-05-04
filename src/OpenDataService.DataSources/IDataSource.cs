namespace OpenDataService.DataSources;

public interface IEntitySet
{
    string Name { get; }
    Type ClrType { get; }
    IQueryable Get();
}

public interface IEntitySet<T> : IEntitySet
{
    IQueryable<T> GetClr();
}

public interface IDataSource : IEnumerable<IEntitySet>
{
    public IEntitySet GetEntitySet(string name);
}