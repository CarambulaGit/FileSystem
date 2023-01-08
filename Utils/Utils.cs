using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class Utils { }

    public static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) => collection == null || !collection.Any();

        public static bool ContentsMatch<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first.IsNullOrEmpty() && second.IsNullOrEmpty()) return true;
            if (first.IsNullOrEmpty() || second.IsNullOrEmpty()) return false;

            var firstCount = first.Count();
            var secondCount = second.Count();
            if (firstCount != secondCount) return false;

            foreach (var x1 in first)
            {
                if (!second.Contains(x1)) return false;
            }

            return true;
        }

        public static void FillWith<T>(this T[] array, Func<T> elemToFill)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = elemToFill.Invoke();
        }

        public static void FillWith<T>(this T[] array, Func<int, T> elemToFill)
        {
            for (var i = 0; i < array.Length; i++)
                array[i] = elemToFill.Invoke(i);
        }

        public static T FirstOrValue<T>(this IEnumerable<T> enumerable, Predicate<T> predicate, T valueIfNotFound)
        {
            foreach (var elem in enumerable)
            {
                if (predicate.Invoke(elem)) return elem;
            }

            return valueIfNotFound;
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, T valueToFound) =>
            enumerable.IndexOf(elem => elem.Equals(valueToFound));

        public static int IndexOf<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            var count = 0;
            foreach (var elem in enumerable)
            {
                if (predicate.Invoke(elem)) return count;
                count++;
            }

            return -1;
        }
    }
}