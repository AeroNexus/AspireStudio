using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aspire.Utilities.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, params T[] items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        public static void AddIfNew<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            IEnumerable<T> newItems = (from x in enumerable
                                       where !collection.Contains(x)
                                       select x);

            foreach (T item in newItems)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Perform ForEach on each element in the <paramref name="collection"/>
        /// </summary>
        /// <typeparam name="T">The collection type</typeparam>
        /// <param name="collection"></param>
        /// <param name="action">The action to perform on each item in the colection</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
          foreach (var item in collection)
            action(item);
        }

        ///// <summary>
        ///// Perform ForEach on each element in the <paramref name="collection"/>
        ///// </summary>
        ///// <typeparam name="T">The collection type</typeparam>
        ///// <param name="collection"></param>
        ///// <param name="action">The action to perform on each item in the colection</param>
        //public static void ForEach<K,T>(this Dictionary<K,T> dictionary, Action<T> action)
        //{
        //  foreach (var item in dictionary.Values)
        //    action(item);
        //}

        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                if (collection.Contains(item))
                {
                    collection.Remove(item);
                }
            }
        }

		  public static T[] ToArray<T>(this ICollection<T> collection)
		  {
			  T[] array = new T[collection.Count];
			  int i = 0;
			  foreach(var item in collection)
				  array[i++] = item;
			  return array;
		  }
    }
}