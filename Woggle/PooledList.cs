using System.Buffers;
using System.Collections;

namespace Woggle;

/// <summary>
/// Represents a list of elements that uses an underlying array rented from an <see cref="ArrayPool{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
/// <remarks>
/// This collection is not thread-safe.
/// </remarks>
public sealed class PooledList<T> : PooledArrayHandler<T>, IList<T>, ICollection<T>
{
    private const int DefaultCapacity = 8;

    /// <inheritdoc/>
    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, nameof(index));
            return Array[index];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, nameof(index));
            Array[index] = value;
        }
    }

    /// <inheritdoc/>
    public int Count { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="PooledList{T}"/> is read-only.
    /// </summary>
    /// <returns>This property always returns <c>false</c>.</returns>
    public bool IsReadOnly => false;

    private Span<T> Items => new(Array, 0, Count);

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty and has a default initial capacity.
    /// </summary>
    public PooledList() : base(DefaultCapacity, ArrayPool<T>.Shared)
    {
        Count = default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    public PooledList(int capacity) : base(capacity, ArrayPool<T>.Shared)
    {
        Count = default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty and has a default initial capacity.
    /// </summary>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to use.</param>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    public PooledList(ArrayPool<T> arrayPool, int capacity = DefaultCapacity) : base(capacity, arrayPool)
    {
        Count = default;
    }

    private void Expand()
    {
        int capacity;
        checked
        {
            capacity = Array.Length * 2;
        }

        Resize(capacity);
    }

    private void Expand(int count)
    {
        int availableSpace = Array.Length - Count;

        if (availableSpace >= count)
        {
            return;
        }

        int newSize;
        checked
        {
            newSize = ((count - availableSpace) * 2) + Count;
        }
        Resize(newSize);
    }

    private void Resize(int capacity)
    {
        T[] array = Pool.Rent(capacity);
        Items.CopyTo(array);
        Array = array;
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (Array.Length <= Count)
        {
            Expand();
        }

        Array[Count++] = item;
    }

    /// <summary>
    /// Adds the elements of the specified <see cref="ReadOnlySpan{T}"/> to the end of the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="items">The <see cref="ReadOnlySpan{T}"/> whose elements should be added to the end of the <see cref="PooledList{T}"/>.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    [Obsolete($"This method is deprecated and will be removed in future versions. Use {nameof(AddRange)} instead.")]
    public void Add(ReadOnlySpan<T> items)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        Expand(items.Length);
        Span<T> destination = new(Array, Count, items.Length);
        items.CopyTo(destination);
        Count += items.Length;
    }

    /// <summary>
    /// Adds the elements of the specified <see cref="ReadOnlySpan{T}"/> to the end of the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="items">The <see cref="ReadOnlySpan{T}"/> whose elements should be added to the end of the <see cref="PooledList{T}"/>.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void AddRange(ReadOnlySpan<T> items)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        Expand(items.Length);
        Span<T> destination = new(Array, Count, items.Length);
        items.CopyTo(destination);
        Count += items.Length;
    }

    /// <summary>
    /// Adds the elements of the specified <see cref="ICollection{T}"/> to the end of the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="items">The <see cref="ICollection{T}"/> whose elements should be added to the end of the <see cref="PooledList{T}"/>.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void AddRange(ICollection<T> items)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        Expand(items.Count);
        items.CopyTo(Array, Count);
        Count += items.Count;
    }

    /// <summary>
    /// Adds the elements of the specified <see cref="IEnumerable{T}"/> to the end of the <see cref="PooledList{T}"/>.
    /// </summary>
    /// <param name="items">The <see cref="IEnumerable{T}"/> whose elements should be added to the end of the <see cref="PooledList{T}"/>.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void AddRange(IEnumerable<T> items)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        foreach (T item in items)
        {
            Add(item);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (t_isReferenceOrContainsReferences)
        {
            Items.Clear();
        }

        Count = default;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public bool Contains(T item)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return IndexOf(item) != -1;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        Array.AsSpan(0, Count).CopyTo(array.AsSpan(arrayIndex));
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public int IndexOf(T item)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        for (int i = 0; i < Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(Array[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void Insert(int index, T item)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count, nameof(index));

        if (Array.Length <= Count)
        {
            Expand();
        }

        if (index < Count)
        {
            ReadOnlySpan<T> source = Array.AsSpan(index, Count - index);
            Span<T> destination = Array.AsSpan(index + 1, Count - index);
            source.CopyTo(destination);
        }

        Array[index] = item;
        Count++;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public bool Remove(T item)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        int index = _indexOf(Array, item, Count);
        if (index == -1)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public void RemoveAt(int index)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, nameof(index));

        if (index < Count--)
        {
            var source = Array.AsSpan(index + 1, Count - index);
            var destination = Array.AsSpan(index, Count - index);
            source.CopyTo(destination);
        }

        Array[Count] = default!;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledList{T}"/> has been disposed.</exception>
    public IEnumerator<T> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return EnumerateContents(Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal static PooledList<T> FromCollection(ICollection<T> values, ArrayPool<T> arrayPool)
    {
        PooledList<T> list = new(arrayPool, values.Count);
        values.CopyTo(list.Array, 0);
        list.Count = values.Count;
        return list;
    }

    internal static PooledList<T> FromSpan(ReadOnlySpan<T> values, ArrayPool<T> arrayPool)
    {
        PooledList<T> list = new(arrayPool, values.Length);
        values.CopyTo(list.Array);
        list.Count = values.Length;
        return list;
    }
}
