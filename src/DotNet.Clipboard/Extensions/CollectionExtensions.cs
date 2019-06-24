namespace DotNet.Clipboard.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This represents the extension entity for the <see cref="ICollection{TSource}" /> class.
    /// </summary>
    public static class CollectionExtensions
    {
        public static void AddRange<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (var item in collection)
            {
                source.Add(item);
            }
        }

        public static void RemoveRange<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (var item in collection)
            {
                source.Remove(item);
            }
        }

        public static void Reset<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            source.Clear();

            foreach (var item in collection)
            {
                source.Add(item);
            }
        }
    }
}
