using OpenDataService.DataSources;
using System.Reflection;

namespace OpenDataService.DataSources.Dynamic;

public static class CloneExtension
{
    public static IEnumerable<T> Clone<T>(this IEntitySet entitySet)
    {
        var sourceFieldsAndProperties = entitySet.ClrType.GetFieldsAndProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        var targetFieldsAndProperties = typeof(T).GetFieldsAndProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

        var fieldsMapping = sourceFieldsAndProperties.Select(sourceField => {
            var targetField = targetFieldsAndProperties.SingleOrDefault(t => t.Name == sourceField.Name);
            if (targetField == null)
            {
                throw new Exception(string.Format("Type {0} has no field named {1}", typeof(T), sourceField.Name));
            }
            if (!targetField.GetMemberType().IsAssignableFrom(sourceField.GetMemberType()))
            {
                throw new Exception(string.Format("Field {0} cannot be copied because type {1} cannot be assigned to type {2}", sourceField.Name, sourceField.GetMemberType(), targetField.GetMemberType()));
            }
            return new { Source=sourceField, Target=targetField };
        }).ToList();

        foreach (var item in entitySet.Get())
        {
            T targetItem = (T)Activator.CreateInstance(typeof(T))!;
            foreach (var field in fieldsMapping)
            {
                var value = field.Source.GetMemberValue(item);
                field.Target.SetMemberValue(targetItem, value);
            }
            yield return targetItem;
        }
    }

    public static object? GetMemberValue(this MemberInfo member, object instance)
    {
        var fieldInfo = member as FieldInfo;
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(instance);
        }

        var propertyInfo = member as PropertyInfo;
        if (propertyInfo != null)
        {
            return propertyInfo.GetValue(instance);
        }

        throw new Exception();
    }

    public static void SetMemberValue(this MemberInfo member, object instance, object? value)
    {
        var fieldInfo = member as FieldInfo;
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(instance, value);
            return;
        }

        var propertyInfo = member as PropertyInfo;
        if (propertyInfo != null)
        {
            propertyInfo.SetValue(instance, value);
            return;
        }

        throw new Exception();
    }

    public static Type GetMemberType(this MemberInfo member)
    {
        var fieldInfo = member as FieldInfo;
        if (fieldInfo != null)
        {
            return fieldInfo.FieldType;
        }

        var propertyInfo = member as PropertyInfo;
        if (propertyInfo != null)
        {
            return propertyInfo.PropertyType;
        }

        throw new Exception();
    }

    public static IEnumerable<MemberInfo> GetFieldsAndProperties(this Type type, BindingFlags bindingFlags)
    {
        foreach (var field in type.GetFields(bindingFlags))
        {
            yield return field;
        }
        foreach (var field in type.GetProperties(bindingFlags))
        {
            yield return field;
        }
    }
}