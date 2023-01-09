using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Utils
{
    public static class Utils { }

    public static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) => collection == null || !collection.Any();

        public static bool ContentsMatch<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var firstNullOrEmpty = first.IsNullOrEmpty();
            var secondNullOrEmpty = second.IsNullOrEmpty();
            if (firstNullOrEmpty && secondNullOrEmpty) return true;
            if (firstNullOrEmpty || secondNullOrEmpty) return false;

            var firstCount = first.Count();
            var secondCount = second.Count();
            if (firstCount != secondCount) return false;

            foreach (var x1 in first)
            {
                if (!second.Contains(x1)) return false;
            }

            return true;
        }

        public static IEnumerable<T> FirstN<T>(this IEnumerable<T> enumerable, int n, Predicate<T> predicate)
        {
            var enumerator = enumerable.GetEnumerator();
            var result = new List<T>(n);
            while (enumerator.MoveNext() && result.Count < n)
            {
                var curElem = enumerator.Current;
                if (predicate.Invoke(curElem))
                {
                    result.Add(curElem);
                }
            }

            return result;
        }

        public static IEnumerable<int> FirstNIndexes<T>(this IList<T> list, int n, Predicate<T> predicate)
        {
            var result = new List<int>(n);
            for (var i = 0; i < list.Count && result.Count < n; i++)
            {
                if (predicate.Invoke(list[i]))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        public static bool ContentsMatchOrdered<T>(this IList<T> first, IList<T> second)
        {
            if (first.IsNullOrEmpty() && second.IsNullOrEmpty()) return true;
            if (first.Count != second.Count) return false;

            for (var i = 0; i < first.Count; i++)
            {
                if (!second[i].Equals(first[i])) return false;
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

        public static List<T> GetRangeByIndexes<T>(this List<T> list, int startIndex, int endIndex) =>
            list.GetRange(startIndex, endIndex - startIndex + 1);

        public static List<T> GetRangeByEndIndex<T>(this List<T> list, int endIndex) =>
            list.GetRange(0, endIndex + 1);

        public static List<T> GetRangeByStartIndex<T>(this List<T> list, int startIndex) =>
            list.GetRange(startIndex, list.Count - startIndex);
    }
}