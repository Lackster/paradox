﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SiliconStudio.Presentation.Collections
{
    /// <summary>
    /// An observable collection that automatically sorts inserted items using either the default comparer for their type, or a custom provider comparer.
    /// Insertion and search are both O(log(n)). The method <see cref="BinarySearch"/> must be used for O(log(n)).
    /// If items are modified outside from the collection and these changes affect the order, the collection won't be updated. You must remove them from the collection before modifying it, and re-add it afterwards.
    /// Use an <see cref="AutoUpdatingSortedObservableCollection{T}"/> if you want to maintain order after modifying items.
    /// </summary>
    /// <typeparam name="T">The type of item this collection contains.</typeparam>
    /// <seealso cref="AutoUpdatingSortedObservableCollection{T}"/>
    public class SortedObservableCollection<T> : ObservableCollection<T>, IObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        protected readonly Func<T, T, int> DefaultCompareFunc;

        /// <summary>
        /// Public constructor. A comparer can be provided to compare items. If null, the default comparer will be used (if available).
        /// If no comparer is provided and no default comparer is available, an <see cref="InvalidOperationException"/> will be thrown when methods requesting comparer are invoked.
        /// </summary>
        /// <param name="comparer">The comparer to use to compare items.</param>
        public SortedObservableCollection(IComparer<T> comparer = null)
        {
            // Check if a comparer is provided. If so, we will use it for sorting.
            if (comparer != null)
            {
                DefaultCompareFunc = comparer.Compare;
            }

            // Otherwise, let's check if the generic type T implements the generic IComparable interface.
            if (DefaultCompareFunc == null && typeof(T).GetInterfaces().Contains(typeof(IComparable<T>)))
            {
                if (typeof(T).IsValueType)
                {
                    DefaultCompareFunc = (item1, item2) => ((IComparable<T>)item1).CompareTo(item2);
                }
                else
                {
                    DefaultCompareFunc = (item1, item2) =>
                    {
                        // ReSharper disable RedundantCast - If we don't cast, we get a possible compare of a value type with null, which is not the case here
                        if ((object)item1 == null && (object)item2 == null)
                            // ReSharper restore RedundantCast
                            return 0;

                        return (object)item1 != null ? ((IComparable<T>)item1).CompareTo(item2) : -((IComparable<T>)item2).CompareTo(item1);
                    };
                }
            }

            // If we still have no way of comparing objects, let's check for the non-generic IComparable interface.
            if (DefaultCompareFunc == null && typeof(T).GetInterfaces().Contains(typeof(IComparable)))
            {
                if (typeof(T).IsValueType)
                {
                    DefaultCompareFunc = (item1, item2) => ((IComparable)item1).CompareTo(item2);
                }
                else
                {
                    DefaultCompareFunc = (item1, item2) =>
                    {
                        // ReSharper disable RedundantCast - If we don't cast, we get a possible compare of a value type with null, which is not the case here
                        if ((object)item1 == null && (object)item2 == null)
                            // ReSharper restore RedundantCast
                            return 0;

                        return (object)item1 != null ? ((IComparable)item1).CompareTo(item2) : -((IComparable)item2).CompareTo(item1);
                    };
                }
            }

            // If we have no comparer at this point we're dead
            if (DefaultCompareFunc == null)
            {
                throw new ArgumentException("The type of this collection does not implement any IComparable interface and no IComparer has been provided");
            }
        }

        /// <summary>
        /// Seach the index of an item.
        /// </summary>
        /// <param name="item">The item to search.</param>
        /// <returns>The index of the item in the collection, or -1 if the item does not exist in the collection.</returns>
        public int BinarySearch(T item)
        {
            return GetIndex(item, false);
        }

        /// <summary>
        /// Search the index of key in the collection, using a provided function to compare items to the key.
        /// </summary>
        /// <typeparam name="TSearch">The type of the key to search.</typeparam>
        /// <param name="key">The key to search in the collection.</param>
        /// <param name="compareFunc">A comparison function that can compare a key to an item of the collection.</param>
        /// <returns></returns>
        public int BinarySearch<TSearch>(TSearch key, Func<T, TSearch, int> compareFunc)
        {
            if (compareFunc == null)
                throw new ArgumentNullException("compareFunc");

            return GetIndex(key, false, compareFunc);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{SortedObservableCollection}} Count = {0}", Count);
        }
        
        protected int GetIndex(T item, bool returnInsertIndex)
        {
            if (DefaultCompareFunc == null) throw new InvalidOperationException("No comparison available or provided for items of this collection.");
            return GetIndex(item, returnInsertIndex, DefaultCompareFunc);
        }

        protected int GetIndex<TSearch>(TSearch key, bool returnInsertIndex, Func<T, TSearch, int> compareFunc)
        {
            int imin = 0;
            int imax = Count - 1;
            while (imax >= imin)
            {
                int imid = (imin + imax) / 2;

                int comp = compareFunc(this[imid], key);
                if (comp < 0)
                    imin = imid + 1;
                else if (comp > 0)
                    imax = imid - 1;
                else
                    return imid;
            }

            return returnInsertIndex ? imin : -1;
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            int i = GetIndex(item, true);
            base.InsertItem(i, item);
        }

        /// <inheritdoc/>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new InvalidOperationException("Cannot call MoveItem on a SortedObservableCollection");
        }

        protected void ObservableCollectionMoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, T item)
        {
            throw new InvalidOperationException("Cannot call SetItem on a SortedObservableCollection");
        }
    }
}
