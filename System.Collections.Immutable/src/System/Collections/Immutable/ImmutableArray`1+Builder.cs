#if !NET45PLUS

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Validation;

namespace System.Collections.Immutable
{
    public partial struct ImmutableArray<T>
    {
        /// <summary>
        /// A writable array accessor that can be converted into an <see cref="ImmutableArray{T}"/>
        /// instance without allocating memory.
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableArrayBuilderDebuggerProxy<>))]
        public sealed class Builder : IList<T>, IReadOnlyList<T>
        {
            /// <summary>
            /// The backing array for the builder.
            /// </summary>
            private T[] _elements;

            /// <summary>
            /// The number of initialized elements in the array.
            /// </summary>
            private int _count;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="capacity">The initial capacity of the internal array.</param>
            internal Builder(int capacity)
            {
                Requires.Range(capacity >= 0, "capacity");
                _elements = new T[capacity];
                _count = 0;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            internal Builder()
                : this(8)
            {
            }

            /// <summary>
            /// Get and sets the length of the internal array.  When set the internal array is
            /// reallocated to the given capacity if it is not already the specified length.
            /// </summary>
            public int Capacity
            {
                get { return _elements.Length; }
                set
                {
                    if (value < _count)
                    {
                        throw new ArgumentException(SR.CapacityMustBeGreaterThanOrEqualToCount, paramName: "value");
                    }

                    if (value != _elements.Length)
                    {
                        if (value > 0)
                        {
                            var temp = new T[value];
                            if (_count > 0)
                            {
                                Array.Copy(_elements, 0, temp, 0, _count);
                            }

                            _elements = temp;
                        }
                        else
                        {
                            _elements = ImmutableArray<T>.Empty.array;
                        }
                    }
                }
            }

            /// <summary>
            /// Gets or sets the length of the builder.
            /// </summary>
            /// <remarks>
            /// If the value is decreased, the array contents are truncated.
            /// If the value is increased, the added elements are initialized to the default value of type <typeparamref name="T"/>.
            /// </remarks>
            public int Count
            {
                get
                {
                    return _count;
                }

                set
                {
                    Requires.Range(value >= 0, "value");
                    if (value < _count)
                    {
                        // truncation mode
                        // Clear the elements of the elements that are effectively removed.

                        // PERF: Array.Clear works well for big arrays, 
                        //       but may have too much overhead with small ones (which is the common case here)
                        if (_count - value > 64)
                        {
                            Array.Clear(_elements, value, _count - value);
                        }
                        else
                        {
                            for (int i = value; i < this.Count; i++)
                            {
                                _elements[i] = default(T);
                            }
                        }
                    }
                    else if (value > _count)
                    {
                        // expansion
                        this.EnsureCapacity(value);
                    }

                    _count = value;
                }
            }

            /// <summary>
            /// Gets or sets the element at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns></returns>
            /// <exception cref="System.IndexOutOfRangeException">
            /// </exception>
            public T this[int index]
            {
                get
                {
                    if (index >= this.Count)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return _elements[index];
                }

                set
                {
                    if (index >= this.Count)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    _elements[index] = value;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.
            ///   </returns>
            bool ICollection<T>.IsReadOnly
            {
                get { return false; }
            }

            /// <summary>
            /// Returns an immutable copy of the current contents of this collection.
            /// </summary>
            /// <returns>An immutable array.</returns>
            public ImmutableArray<T> ToImmutable()
            {
                if (Count == 0)
                {
                    return Empty;
                }

                return new ImmutableArray<T>(this.ToArray());
            }

            /// <summary>
            /// Extracts the internal array as an <see cref="ImmutableArray{T}"/> and replaces it 
            /// with a zero length array.
            /// </summary>
            /// <exception cref="InvalidOperationException">When <see cref="ImmutableArray{T}.Builder.Count"/> doesn't 
            /// equal <see cref="ImmutableArray{T}.Builder.Capacity"/>.</exception>
            public ImmutableArray<T> MoveToImmutable()
            {
                if (Capacity != Count)
                {
                    throw new InvalidOperationException(SR.CapacityMustEqualCountOnMove);
                }

                T[] temp = _elements;
                _elements = ImmutableArray<T>.Empty.array;
                _count = 0;
                return new ImmutableArray<T>(temp);
            }

            /// <summary>
            /// Removes all items from the <see cref="ICollection{T}"/>.
            /// </summary>
            public void Clear()
            {
                this.Count = 0;
            }

            /// <summary>
            /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
            /// <param name="item">The object to insert into the <see cref="IList{T}"/>.</param>
            public void Insert(int index, T item)
            {
                Requires.Range(index >= 0 && index <= this.Count, "index");
                this.EnsureCapacity(this.Count + 1);

                if (index < this.Count)
                {
                    Array.Copy(_elements, index, _elements, index + 1, this.Count - index);
                }

                _count++;
                _elements[index] = item;
            }

            /// <summary>
            /// Adds an item to the <see cref="ICollection{T}"/>.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
            public void Add(T item)
            {
                this.EnsureCapacity(this.Count + 1);
                _elements[_count++] = item;
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            public void AddRange(IEnumerable<T> items)
            {
                Requires.NotNull(items, "items");

                int count;
                if (items.TryGetCount(out count))
                {
                    this.EnsureCapacity(this.Count + count);
                }

                foreach (var item in items)
                {
                    this.Add(item);
                }
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            public void AddRange(params T[] items)
            {
                Requires.NotNull(items, "items");

                var offset = this.Count;
                this.Count += items.Length;

                var nodes = _elements;
                for (int i = 0; i < items.Length; i++)
                {
                    nodes[offset + i] = items[i];
                }
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            public void AddRange<TDerived>(TDerived[] items) where TDerived : T
            {
                Requires.NotNull(items, "items");

                var offset = this.Count;
                this.Count += items.Length;

                var nodes = _elements;
                for (int i = 0; i < items.Length; i++)
                {
                    nodes[offset + i] = items[i];
                }
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            /// <param name="length">The number of elements from the source array to add.</param>
            public void AddRange(T[] items, int length)
            {
                Requires.NotNull(items, "items");
                Requires.Range(length >= 0, "length");

                var offset = this.Count;
                this.Count += length;

                var nodes = _elements;
                for (int i = 0; i < length; i++)
                {
                    nodes[offset + i] = items[i];
                }
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            public void AddRange(ImmutableArray<T> items)
            {
                this.AddRange(items, items.Length);
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            /// <param name="length">The number of elements from the source array to add.</param>
            public void AddRange(ImmutableArray<T> items, int length)
            {
                Requires.Range(length >= 0, "length");

                if (items.array != null)
                {
                    this.AddRange(items.array, length);
                }
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            public void AddRange<TDerived>(ImmutableArray<TDerived> items) where TDerived : T
            {
                if (items.array != null)
                {
                    this.AddRange(items.array);
                }
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            public void AddRange(Builder items)
            {
                Requires.NotNull(items, "items");
                this.AddRange(items._elements, items.Count);
            }

            /// <summary>
            /// Adds the specified items to the end of the array.
            /// </summary>
            /// <param name="items">The items.</param>
            public void AddRange<TDerived>(ImmutableArray<TDerived>.Builder items) where TDerived : T
            {
                Requires.NotNull(items, "items");
                this.AddRange(items._elements, items.Count);
            }

            /// <summary>
            /// Removes the specified element.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <returns>A value indicating whether the specified element was found and removed from the collection.</returns>
            public bool Remove(T element)
            {
                int index = this.IndexOf(element);
                if (index >= 0)
                {
                    this.RemoveAt(index);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Removes the <see cref="IList{T}"/> item at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the item to remove.</param>
            public void RemoveAt(int index)
            {
                Requires.Range(index >= 0 && index < this.Count, "index");

                if (index < this.Count - 1)
                {
                    Array.Copy(_elements, index + 1, _elements, index, this.Count - index - 1);
                }

                this.Count--;
            }

            /// <summary>
            /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
            /// <returns>
            /// true if <paramref name="item"/> is found in the <see cref="ICollection{T}"/>; otherwise, false.
            /// </returns>
            public bool Contains(T item)
            {
                return this.IndexOf(item) >= 0;
            }

            /// <summary>
            /// Creates a new array with the current contents of this Builder.
            /// </summary>
            public T[] ToArray()
            {
                var tmp = new T[this.Count];
                var elements = _elements;
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = elements[i];
                }

                return tmp;
            }

            /// <summary>
            /// Copies the current contents to the specified array.
            /// </summary>
            /// <param name="array">The array to copy to.</param>
            /// <param name="index">The starting index of the target array.</param>
            public void CopyTo(T[] array, int index)
            {
                Requires.NotNull(array, "array");
                Requires.Range(index >= 0 && index + this.Count <= array.Length, "start");
                Array.Copy(_elements, 0, array, index, this.Count);
            }

            /// <summary>
            /// Resizes the array to accommodate the specified capacity requirement.
            /// </summary>
            /// <param name="capacity">The required capacity.</param>
            private void EnsureCapacity(int capacity)
            {
                if (_elements.Length < capacity)
                {
                    int newCapacity = Math.Max(_elements.Length * 2, capacity);
                    Array.Resize(ref _elements, newCapacity);
                }
            }

            /// <summary>
            /// Determines the index of a specific item in the <see cref="IList{T}"/>.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="IList{T}"/>.</param>
            /// <returns>
            /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
            /// </returns>
            [Pure]
            public int IndexOf(T item)
            {
                return this.IndexOf(item, 0, _count, EqualityComparer<T>.Default);
            }

            /// <summary>
            /// Searches the array for the specified item.
            /// </summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            [Pure]
            public int IndexOf(T item, int startIndex)
            {
                return this.IndexOf(item, startIndex, this.Count - startIndex, EqualityComparer<T>.Default);
            }

            /// <summary>
            /// Searches the array for the specified item.
            /// </summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            [Pure]
            public int IndexOf(T item, int startIndex, int count)
            {
                return this.IndexOf(item, startIndex, count, EqualityComparer<T>.Default);
            }

            /// <summary>
            /// Searches the array for the specified item.
            /// </summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            [Pure]
            public int IndexOf(T item, int startIndex, int count, IEqualityComparer<T> equalityComparer)
            {
                Requires.NotNull(equalityComparer, "equalityComparer");

                if (count == 0 && startIndex == 0)
                {
                    return -1;
                }

                Requires.Range(startIndex >= 0 && startIndex < this.Count, "startIndex");
                Requires.Range(count >= 0 && startIndex + count <= this.Count, "count");

                if (equalityComparer == EqualityComparer<T>.Default)
                {
                    return Array.IndexOf(_elements, item, startIndex, count);
                }
                else
                {
                    for (int i = startIndex; i < startIndex + count; i++)
                    {
                        if (equalityComparer.Equals(_elements[i], item))
                        {
                            return i;
                        }
                    }

                    return -1;
                }
            }

            /// <summary>
            /// Searches the array for the specified item in reverse.
            /// </summary>
            /// <param name="item">The item to search for.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            [Pure]
            public int LastIndexOf(T item)
            {
                if (this.Count == 0)
                {
                    return -1;
                }

                return this.LastIndexOf(item, this.Count - 1, this.Count, EqualityComparer<T>.Default);
            }

            /// <summary>
            /// Searches the array for the specified item in reverse.
            /// </summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            [Pure]
            public int LastIndexOf(T item, int startIndex)
            {
                if (this.Count == 0 && startIndex == 0)
                {
                    return -1;
                }

                Requires.Range(startIndex >= 0 && startIndex < this.Count, "startIndex");

                return this.LastIndexOf(item, startIndex, startIndex + 1, EqualityComparer<T>.Default);
            }

            /// <summary>
            /// Searches the array for the specified item in reverse.
            /// </summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            [Pure]
            public int LastIndexOf(T item, int startIndex, int count)
            {
                return this.LastIndexOf(item, startIndex, count, EqualityComparer<T>.Default);
            }

            /// <summary>
            /// Searches the array for the specified item in reverse.
            /// </summary>
            /// <param name="item">The item to search for.</param>
            /// <param name="startIndex">The index at which to begin the search.</param>
            /// <param name="count">The number of elements to search.</param>
            /// <param name="equalityComparer">The equality comparer to use in the search.</param>
            /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
            [Pure]
            public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T> equalityComparer)
            {
                Requires.NotNull(equalityComparer, "equalityComparer");

                if (count == 0 && startIndex == 0)
                {
                    return -1;
                }

                Requires.Range(startIndex >= 0 && startIndex < this.Count, "startIndex");
                Requires.Range(count >= 0 && startIndex - count + 1 >= 0, "count");

                if (equalityComparer == EqualityComparer<T>.Default)
                {
                    return Array.LastIndexOf(_elements, item, startIndex, count);
                }
                else
                {
                    for (int i = startIndex; i >= startIndex - count + 1; i--)
                    {
                        if (equalityComparer.Equals(item, _elements[i]))
                        {
                            return i;
                        }
                    }

                    return -1;
                }
            }

            /// <summary>
            /// Reverses the order of elements in the collection.
            /// </summary>
            public void Reverse()
            {
                Array.Reverse(_elements, 0, _count);
            }

            /// <summary>
            /// Sorts the array.
            /// </summary>
            public void Sort()
            {
                if (Count > 1)
                {
                    Array.Sort(_elements, 0, this.Count, Comparer<T>.Default);
                }
            }

            /// <summary>
            /// Sorts the array.
            /// </summary>
            /// <param name="comparer">The comparer to use in sorting. If <c>null</c>, the default comparer is used.</param>
            public void Sort(IComparer<T> comparer)
            {
                if (Count > 1)
                {
                    Array.Sort(_elements, 0, _count, comparer);
                }
            }

            /// <summary>
            /// Sorts the array.
            /// </summary>
            /// <param name="index">The index of the first element to consider in the sort.</param>
            /// <param name="count">The number of elements to include in the sort.</param>
            /// <param name="comparer">The comparer to use in sorting. If <c>null</c>, the default comparer is used.</param>
            public void Sort(int index, int count, IComparer<T> comparer)
            {
                // Don't rely on Array.Sort's argument validation since our internal array may exceed
                // the bounds of the publicly addressable region.
                Requires.Range(index >= 0, "index");
                Requires.Range(count >= 0 && index + count <= this.Count, "count");

                if (count > 1)
                {
                    Array.Sort(_elements, index, count, comparer);
                }
            }

            /// <summary>
            /// Returns an enumerator for the contents of the array.
            /// </summary>
            /// <returns>An enumerator.</returns>
            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < this.Count; i++)
                {
                    yield return this[i];
                }
            }

            /// <summary>
            /// Returns an enumerator for the contents of the array.
            /// </summary>
            /// <returns>An enumerator.</returns>
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator for the contents of the array.
            /// </summary>
            /// <returns>An enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            /// <summary>
            /// Adds items to this collection.
            /// </summary>
            /// <typeparam name="TDerived">The type of source elements.</typeparam>
            /// <param name="items">The source array.</param>
            /// <param name="length">The number of elements to add to this array.</param>
            private void AddRange<TDerived>(TDerived[] items, int length) where TDerived : T
            {
                this.EnsureCapacity(this.Count + length);

                var offset = this.Count;
                this.Count += length;

                var nodes = _elements;
                for (int i = 0; i < length; i++)
                {
                    nodes[offset + i] = items[i];
                }
            }
        }
    }

    /// <summary>
    /// A simple view of the immutable collection that the debugger can show to the developer.
    /// </summary>
    internal sealed class ImmutableArrayBuilderDebuggerProxy<T>
    {
        /// <summary>
        /// The collection to be enumerated.
        /// </summary>
        private readonly ImmutableArray<T>.Builder _builder;
        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableArrayBuilderDebuggerProxy{T}"/> class.
        /// </summary>
        /// <param name="builder">The collection to display in the debugger</param>
        public ImmutableArrayBuilderDebuggerProxy(ImmutableArray<T>.Builder builder)
        {
            _builder = builder;
        }

        /// <summary>
        /// Gets a simple debugger-viewable collection.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] A
        {
            get
            {
                return _builder.ToArray();
            }
        }
    }
}

#endif
