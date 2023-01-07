using System.Collections.Generic;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Internal
{
    static class DictionaryExtensions
    {
        public static TDictionary MergeNoOverride<TDictionary, TKey, TValue>(
            this TDictionary self, [NotNull] IDictionary<TKey, TValue> dictionary)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var entry in dictionary)
            {
                if (self.ContainsKey(entry.Key))
                    continue;

                self[entry.Key] = entry.Value;
            }

            return self;
        }

        public static TDictionary MergeAllowOverride<TDictionary, TKey, TValue>(
            this TDictionary self, [NotNull] IDictionary<TKey, TValue> dictionary)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var entry in dictionary)
            {
                self[entry.Key] = entry.Value;
            }

            return self;
        }

        public static bool ValueEquals<TKey, TValue>(this IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
            => ValueEquals(x, y, EqualityComparer<TValue>.Default);

        public static bool ValueEquals<TKey, TValue, TComparer>(
            this IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y, TComparer valueComparer)
            where TComparer : IEqualityComparer<TValue>
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null)
                || ReferenceEquals(y, null)
                || x.Count != y.Count)
            {
                return false;
            }

            foreach (var kvp in x)
            {
                if (!y.TryGetValue(kvp.Key, out var value2)
                    || !valueComparer.Equals(kvp.Value, value2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
