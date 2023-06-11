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
                DefineProperty(propertyDefinition, typeBuilder);
            }

            var type = typeBuilder.CreateType();
            typemap.Add(typeDefinition, type);
        }

        return typemap;
    }

    private void DefineProperty(PropertyDefinition propertyDefinition, TypeBuilder typeBuilder)
    {
        var fieldBuilder = typeBuilder.DefineField("_" + propertyDefinition.Name, propertyDefinition.Type, FieldAttributes.Private);
        var propertyBuilder = typeBuilder.DefineProperty(propertyDefinition.Name, PropertyAttributes.None, propertyDefinition.Type, null);

        var getterSetterAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        var getterBuilder = typeBuilder.DefineMethod("get_" + propertyDefinition.Name, getterSetterAttributes, propertyDefinition.Type, Type.EmptyTypes);
        var getterIL = getterBuilder.GetILGenerator();
        getterIL.Emit(OpCodes.Ldarg_0);
        getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getterIL.Emit(OpCodes.Ret);

        var setterBuilder = typeBuilder.DefineMethod("set_" + propertyDefinition.Name, getterSetterAttributes, null, new [] { propertyDefinition.Type });
        ILGenerator setterIL = setterBuilder.GetILGenerator();
        setterIL.Emit(OpCodes.Ldarg_0);
        setterIL.Emit(OpCodes.Ldarg_1);
        setterIL.Emit(OpCodes.Stfld, fieldBuilder);
        setterIL.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getterBuilder);
        propertyBuilder.SetSetMethod(setterBuilder);
    }
}