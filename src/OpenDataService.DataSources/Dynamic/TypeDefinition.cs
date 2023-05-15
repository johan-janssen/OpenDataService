namespace OpenDataService.DataSources.Dynamic;

public class TypeDefinition
{
    public TypeDefinition(string name, IEnumerable<PropertyDefinition> properties)
    {
        Name = name;
        Properties = properties;
    }

    public string Name { get; }
    public IEnumerable<PropertyDefinition> Properties { get; }
}