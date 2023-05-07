using System.Collections.Generic;

namespace OpenDataService.Web;

public static class IDictionaryExtensions
{
    public static bool Do<T, TKey, TValue>(this IDictionary<TKey, TValue> dict, T v)
    {
        return false;
    }
    public static bool TryGetValueTyped<T, TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out T? result) where T : class
    {
        TValue? v;
        if (!dict.TryGetValue(key, out v) || (v == null) || !(v is T))
        {
            result = default(T);
            return false;
        }

        result = v as T;
        return true;
    }
}