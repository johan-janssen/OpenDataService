using System.Linq;

namespace OpenDataService.DataSources.Extensions;
public static class IEnumberableExtension
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}