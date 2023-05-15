using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace OpenDataService.DataSources.Dynamic;

public class TypeGenerator
{
    public Dictionary<TypeDefinition, Type> Generate(AssemblyDefinition assemblyDefinition)
    {
        var typemap = new Dictionary<TypeDefinition, Type>();
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyDefinition.Name), AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyBuilder.GetName().ToString());

        foreach (var typeDefinition in assemblyDefinition.Types)
        {
            var typeBuilder = moduleBuilder.DefineType(string.Format("{0}.{1}", assemblyDefinition.Name, typeDefinition.Name), TypeAttributes.Public);

            foreach (var propertyDefinition in typeDefinition.Properties)
            {
                // we generate a field instead of a property because it's easier. Properties need getter and setter methods, which result in a load of code.
                var fieldBuilder = typeBuilder.DefineField(propertyDefinition.Name, propertyDefinition.Type, FieldAttributes.Public);
            }

            var type = typeBuilder.CreateType();
            typemap.Add(typeDefinition, type);
        }

        return typemap;
    }
}