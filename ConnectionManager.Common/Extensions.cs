using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionManager.Common
{
    public static class Extensions
    {
        public static TOut Get<TOut, TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TOut defaultValue = default)
        {
            // If dictionary is null or key is invalid, return default.
            if (dictionary == null || key == null)
            {
                return defaultValue;
            }

            // If setting doesn't exist, create it.
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = (TValue)(object)defaultValue;
            }

            return (TOut)(object)dictionary[key];
        }
    }
}
