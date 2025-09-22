using System.Buffers;
using System.Collections;

namespace Woggle;

/// <summary>
/// Represents a pooled array that uses an <see cref="ArrayPool{T}"/> for memory management.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
/// <remarks>
/// This collection is not thread-safe. You must call <see cref="Dispose"/> to return the underlying array to the pool.
/// Failure to do so will result in a memory leak.
/// </remarks>
public readonly struct PooledArray<T> : IReadOnlyList<T>, IDisposable
{
    private readonly PooledArrayHandle<T> _handle;

    /// <summary>
    /// Gets a new <see cref="PooledArray{T}"/> that represents a slice of the current instance.
    /// </summary>
    /// <param name="range">The range of elements to include in the new array.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the start or end of the range is out of bounds.</exception>
    public readonly PooledArray<T> this[Range range]
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
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is a negative value or is greater than or equal to <see cref="Count"/>.</exception>
    public readonly T this[int index]
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
    /// Gets the number of elements contained in the <see cref="PooledArray{T}"/>.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledArray{T}"/> struct with a specified length, renting from the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <param name="length">The number of elements in the array.</param>
    public PooledArray(int length)
    {
        _handle = new(length, ArrayPool<T>.Shared);
        Count = length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledArray{T}"/> struct by copying elements from a specified <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="items">The span containing the elements to copy.</param>
    public PooledArray(ReadOnlySpan<T> items) : this(items.Length)
    {
        items.CopyTo(_handle.Array);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledArray{T}"/> struct with a specified length, renting from a provided <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <param name="length">The number of elements in the array.</param>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to rent the array from.</param>
    public PooledArray(int length, ArrayPool<T> arrayPool)
    {
        _handle = new(length, arrayPool);
        Count = length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledArray{T}"/> struct by copying elements from a specified <see cref="ReadOnlySpan{T}"/> and renting from a provided <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <param name="items">The span containing the elements to copy.</param>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to rent the array from.</param>
    public PooledArray(ReadOnlySpan<T> items, ArrayPool<T> arrayPool) : this(items.Length, arrayPool)
    {
        items.CopyTo(_handle.Array);
    }

    internal PooledArray(PooledArrayHandle<T> handle, int count)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        Count = count;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="PooledArray{T}"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the array.</returns>
    public readonly IEnumerator<T> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        T[] array = _handle.Array;
        for (int i = 0; i < Count; i++)
        {
            yield return array[i];
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="PooledArray{T}"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the array.</returns>
    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Creates a new span over the target array.
    /// </summary>
    public Span<T> AsSpan()
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        return new(_handle.Array, 0, Count);
    }

    /// <summary>
    /// Creates a new Span over the portion of the target array beginning
    /// at 'start' index and ending at 'end' index (exclusive).
    /// </summary>
    /// <param name="start">The index at which to begin the Span.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
    /// </exception>
    public Span<T> AsSpan(int start)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(start, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, Count, nameof(start));
        return new(_handle.Array, start, Count);
    }

    /// <summary>
    /// Creates a new Span over the portion of the target array beginning
    /// at 'start' index and ending at 'end' index (exclusive).
    /// </summary>
    /// <param name="start">The index at which to begin the Span.</param>
    /// <param name="length">The number of items in the Span.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
    /// </exception>
    public Span<T> AsSpan(int start, int length)
    {
        ObjectDisposedException.ThrowIf(_handle.Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(start, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, Count, nameof(start));
        ArgumentOutOfRangeException.ThrowIfNegative(length, nameof(length));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(length, Count, nameof(length));
        ArgumentOutOfRangeException.ThrowIfLessThan(length, start, nameof(length));
        return new(_handle.Array, start, length);
    }

    /// <summary>
    /// Implicitly converts a <see cref="PooledArray{T}"/> to a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="pooledArray">The pooled array to convert.</param>
    public static implicit operator Span<T>(PooledArray<T> pooledArray)
    {
        return pooledArray.AsSpan();
    }

    /// <summary>
    /// Implicitly converts a <see cref="PooledArray{T}"/> to a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="pooledArray">The pooled array to convert.</param>
    public static implicit operator ReadOnlySpan<T>(PooledArray<T> pooledArray)
    {
        return pooledArray.AsSpan();
    }

    /// <summary>
    /// Returns the rented array to the pool.
    /// </summary>
    public readonly void Dispose()
    {
        _handle.Dispose();
    }
}