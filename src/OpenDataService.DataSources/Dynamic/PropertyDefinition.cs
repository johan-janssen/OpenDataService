using System.Collections.Generic;

namespace OpenDataService.DataSources.Dynamic;

public class PropertyDefinition
{
    public PropertyDefinition(string name, Type type)
    {
        Name = name;
        Type = type;
    }
    public string Name { get; }
    public Type Type { get; }
}