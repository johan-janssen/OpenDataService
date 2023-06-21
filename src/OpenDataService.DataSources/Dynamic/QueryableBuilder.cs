using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OpenDataService.DataSources.Dynamic;

public static class QueryableBuilder
{
    public static IQueryable Build(Type type, object[] objects)
    {
        var generic = typeof(List<>);
        var specific = generic.MakeGenericType(new [] { type });
        var backingList = (IList)Activator.CreateInstance(specific)!;
        foreach (var obj in objects)
        {
            backingList.Add(obj);
        }
        return backingList.AsQueryable();
    }
}