using System.Collections.Generic;

namespace OpenDataService.DataSources.Dynamic;

public class AssemblyDefinition
{
    public AssemblyDefinition(string name, IEnumerable<TypeDefinition> types)
    {
        Name = name;
        Types = types;
    }

    public string Name { get; }
    public IEnumerable<TypeDefinition> Types { get; }
}