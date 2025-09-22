using System.Buffers;
using System.Collections;

namespace Woggle;

/// <summary>
/// Represents a list of elements that uses an underlying array rented from an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <remarks>
/// This collection is not thread-safe. You must call <see cref="Dispose"/> to return the underlying array to the pool.
/// Failure to do so will result in a memory leak.
/// </remarks>
public sealed class PooledList<T> : IList<T>, IDisposable
{
    private readonly PooledArrayHandle<T> _handle;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <value>The element at the specified index.</value>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// -or-
    /// <paramref name="index"/> is equal to or greater than <see cref="Count"/>.
    /// </exception>
    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, nameof(index));
            return _handle.Array[index];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, nameof(index));
            _handle.Array[index] = value;
        }
    }

    /// <summary>
    /// Gets a new <see cref="PooledArray{T}"/> that represents a slice of the current instance.
    /// </summary>
    /// <param name="range">The range of elements to include in the new array.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the start or end of the range is out of bounds.</exception>
    public PooledArray<T> this[Range range]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(range.Start.Value, nameof(range.Start));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(range.End.Value, Count, nameof(range.End));
            ReadOnlySpan<T> values = _handle.Array.AsSpan(range);
            return new PooledArray<T>(values, _handle.Pool);
        }
    }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="PooledList{T}"/>.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="PooledList{T}"/> is read-only.
    /// </summary>
    /// <returns>This property always returns <c>false</c>.</returns>
    public bool IsReadOnly => false;

    private Span<T> Items => new(_handle.Array, 0, Count);

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1.</exception>
    public PooledList(int capacity = ArrayDefaults.DefaultCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, ArrayDefaults.MinCapacity, nameof(capacity));
        _handle = new PooledArrayHandle<T>(capacity, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty and has a default initial capacity.
    /// </summary>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to use.</param>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    /// <exception cref="ArgumentNullException"><paramref name="arrayPool"/> is null.</exception>
    public PooledList(ArrayPool<T> arrayPool, int capacity = ArrayDefaults.DefaultCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, ArrayDefaults.MinCapacity, nameof(capacity));
        _handle = new PooledArrayHandle<T>(capacity, arrayPool);
    }

    internal PooledList(PooledArrayHandle<T> handle, int count)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        Count = count;
    }

    ~PooledList()
    {
        Dispose(false);
    }

    private void Expand()
    {
        int capacity;
        checked
        {
            capacity = _handle.Array.Length << 1;
        }

        Resize(capacity);
    }

    private void Resize(int capacity)
    {
        T[] array = _handle.Pool.Rent(capacity);
        Items.CopyTo(array);
        _handle.Array = array;
    }

    /// <summary>
    /// Adds an object to the end of the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="item">The object to be added to the end of the <see cref="PooledList{T}"/>. The value can be null for reference types.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void Add(T item)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);

        if (_handle.Array.Length <= Count)
        {
            Expand();
        }

        _handle.Array[Count++] = item;
    }

    /// <summary>
    /// Adds the elements of the specified span to the end of the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="items">The span whose elements should be added to the end of the <see cref="PooledList{T}"/>.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void Add(ReadOnlySpan<T> items)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);

        int minSize = Count + items.Length;
        if (minSize > _handle.Array.Length)
        {
            Resize(minSize);
        }

        Span<T> destination = new(_handle.Array, Count, items.Length);
        items.CopyTo(destination);

        Count += items.Length;
    }

    /// <summary>
    /// Removes all elements from the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);

        if (_handle.TypeIsReferenceOrContainsReferences)
        {
            Items.Clear();
        }

        Count = 0;
    }

    /// <summary>
    /// Determines whether an element is in the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="PooledList{T}"/>. The value can be null for reference types.</param>
    /// <returns><c>true</c> if item is found in the <see cref="PooledList{T}"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public bool Contains(T item)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        return IndexOf(item) != -1;
    }

    /// <summary>
    /// Copies the entire <see cref="PooledList{T}"/> to a compatible one-dimensional array, starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="PooledList{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is negative.
    /// -or-
    /// <paramref name="arrayIndex"/> is greater than <see cref="Count"/>.
    /// </exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, Count, nameof(arrayIndex));

        Items.CopyTo(array.AsSpan(arrayIndex));
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="PooledList{T}"/>. The value can be null for reference types.</param>
    /// <returns>The zero-based index of the first occurrence of <paramref name="item"/> within the entire <see cref="PooledList{T}"/>, if found; otherwise, –1.</returns>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public int IndexOf(T item)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);

        T[] array = _handle.Array;
        for (int i = 0; i < Count; i++)
        {
            if (Equals(array[i], item))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Inserts an element into the <see cref="PooledList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The object to insert. The value can be null for reference types.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// -or-
    /// <paramref name="index"/> is greater than <see cref="Count"/>.
    /// </exception>
    public void Insert(int index, T item)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count, nameof(index));

        T[] array = _handle.Array;

        if (array.Length <= Count)
        {
            Expand();
        }

        if (index < Count)
        {
            ReadOnlySpan<T> source = array.AsSpan(index, Count - index);
            Span<T> destination = array.AsSpan(index + 1, Count - index);
            source.CopyTo(destination);
        }

        array[index] = item;
        Count++;
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="PooledList{T}"/>. The value can be null for reference types.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="item"/> is successfully removed; otherwise, <c>false</c>.
    /// This method also returns <c>false</c> if <paramref name="item"/> was not found in the <see cref="PooledList{T}"/>.
    /// </returns>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public bool Remove(T item)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);

        int index = IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes the element at the specified index of the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// -or-
    /// <paramref name="index"/> is equal to or greater than <see cref="Count"/>.
    /// </exception>
    public void RemoveAt(int index)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, nameof(index));

        T[] array = _handle.Array;

        if (index < Count--)
        {
            var source = array.AsSpan(index + 1, Count - index);
            var destination = array.AsSpan(index, Count - index);
            source.CopyTo(destination);
        }

        array[Count] = default!;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}"/> for the <see cref="PooledList{T}"/>.</returns>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public IEnumerator<T> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        T[] array = _handle.Array;
        for (int i = 0; i < Count; i++)
        {
            yield return array[i];
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void Dispose(bool disposing)
    {
        if (_handle.Disposed)
        {
            return;
        }

        if (disposing)
        {
            // This is the place to clean up other managed IDisposable objects if there were any.
        }

        _handle.Dispose();
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PooledList{T}"/> by returning the underlying rented array to the pool.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
