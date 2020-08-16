using System;
using System.Collections.Generic;
using System.Linq;

namespace Chloroplast.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static T[] SubArray<T>(this IEnumerable<T> collection, int from, int to)
        {
            return collection
                .Skip (from)
                .Take (to - from)
                .ToArray ();
        }

        public static string StringJoinFromSubArray (this IEnumerable<string> lines, string separator, int from, int to)
        {
            var sub = lines.SubArray (from, to);
            return String.Join (separator, sub).Trim ();
        }

        public static T Try<T>(this IDictionary<string, T> dict, string key)
        {
            T val;
            if (dict.TryGetValue (key, out val))
                return val;

            return default (T);
        }
    }
}
