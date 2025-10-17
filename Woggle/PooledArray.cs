using System.Buffers;
using System.Collections;

namespace Woggle;

/// <summary>
/// Represents a pooled array that uses an <see cref="ArrayPool{T}"/> for memory management.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
public sealed class PooledArray<T> : PooledArrayHandler<T>, ICollection<T>, IList<T>, IStructuralEquatable, IStructuralComparable, ICloneable
{
    private const string FixedSizeCollectionMessage = "Collection is of a fixed size.";

    private static readonly Func<T[], T, int, int> _indexOf;

    /// <inheritdoc/>
    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length, nameof(index));
            return Array[index];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length, nameof(index));
            Array[index] = value;
        }
    }

    static PooledArray()
    {
        if (typeof(T).IsAssignableTo(typeof(IComparable<T>)))
        {
            _indexOf = BinarySearch;
            return;
        }

        _indexOf = LinearSearch;
    }

    /// <summary>
    /// Gets the total number of elements in the <see cref="PooledArray{T}"/>.
    /// </summary>
    public int Length { get; }

    int ICollection<T>.Count => Length;

    /// <inheritdoc/>
    public bool IsReadOnly { get; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledArray{T}"/> struct with a specified length, renting from the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <param name="length">The number of elements in the array.</param>
    public PooledArray(int length) : base(length, ArrayPool<T>.Shared)
    {
        Length = length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledArray{T}"/> struct with a specified length, renting from the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to use.</param>
    /// <param name="length">The number of elements in the array.</param>
    public PooledArray(ArrayPool<T> arrayPool, int length) : base(length, arrayPool)
    {
        Length = length;
    }

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException(FixedSizeCollectionMessage);
    }

    void ICollection<T>.Clear()
    {
        throw new NotSupportedException(FixedSizeCollectionMessage);
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException(FixedSizeCollectionMessage);
    }

    void IList<T>.Insert(int index, T item)
    {
        throw new NotSupportedException(FixedSizeCollectionMessage);
    }

    void IList<T>.RemoveAt(int index)
    {
        throw new NotSupportedException(FixedSizeCollectionMessage);
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public bool Contains(T item)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return _indexOf(Array, item, Length) >= 0;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        Span<T> source = new(Array, arrayIndex, Length);
        source.CopyTo(array);
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public int IndexOf(T item)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return _indexOf(Array, item, Length);
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public IEnumerator<T> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return EnumerateContents(Length);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="comparer"/> is null.</exception>
    public bool Equals(object? other, IEqualityComparer comparer)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (other is null)
        {
            return false;
        }

        ArgumentNullException.ThrowIfNull(comparer, nameof(comparer));

        return ReferenceEquals(this, other) || other switch
        {
            IReadOnlyCollection<T> values => Length == values.Count && CompareSequences(this, values, comparer),
            IEnumerable<T> values => CompareSequences(this, values, comparer),
            _ => false
        };
    }

    private static bool CompareSequences(PooledArray<T> left, IEnumerable<T> right, IEqualityComparer comparer)
    {
        var leftEnumerator = left.GetEnumerator();
        var rightEnumerator = right.GetEnumerator();

        while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
        {
            if (!comparer.Equals(leftEnumerator.Current, rightEnumerator.Current))
            {
                return false;
            }
        }
        
        return leftEnumerator.MoveNext() == rightEnumerator.MoveNext();
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="comparer"/> is null.</exception>
    public int GetHashCode(IEqualityComparer comparer)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentNullException.ThrowIfNull(comparer);

        var hashCode = new HashCode();
        for (int i = 0; i < Length; i++)
        {
            hashCode.Add(comparer.GetHashCode(Array[i]!));
        }
        return hashCode.ToHashCode();
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="comparer"/> is null.</exception>
    public int CompareTo(object? other, IComparer comparer)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentNullException.ThrowIfNull(comparer);

        if (other is null)
        {
            return 1;
        }

        if (other is not IEnumerable otherEnumerable)
        {
            throw new ArgumentException("Object must be a collection.", nameof(other));
        }
        
        IEnumerator enumerator = otherEnumerable.GetEnumerator();

        for (int i = 0; i < Length; i++)
        {
            if (!enumerator.MoveNext())
            {
                return 1;
            }

            int result = comparer.Compare(Array[i], enumerator.Current);
            if (result != 0)
            {
                return result;
            }
        }

        if (other is IReadOnlyCollection<T> otherCollection)
        {
            return Length.CompareTo(otherCollection.Count);
        }

        return enumerator.MoveNext() ? -1 : 0;
    }

    /// <inheritdoc/>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public object Clone()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        var newPooledArray = new PooledArray<T>(Pool, Length);
        Array.CopyTo(newPooledArray.Array, 0);
        return newPooledArray;
    }

    /// <summary>
    /// Creates a new span over the target array.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public Span<T> AsSpan()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        return new(Array, 0, Length);
    }

    /// <summary>
    /// Creates a new Span over the portion of the target array beginning
    /// at 'start' index and ending at 'end' index (exclusive).
    /// </summary>
    /// <param name="start">The index at which to begin the Span.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
    /// </exception>
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public Span<T> AsSpan(int start)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(start, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, Length, nameof(start));
        return new(Array, start, Length);
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
    /// <exception cref="ObjectDisposedException">The <see cref="PooledArray{T}"/> has been disposed.</exception>
    public Span<T> AsSpan(int start, int length)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(start, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(start, Length, nameof(start));
        ArgumentOutOfRangeException.ThrowIfNegative(length, nameof(length));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(length, Length, nameof(length));
        ArgumentOutOfRangeException.ThrowIfLessThan(length, start, nameof(length));
        return new(Array, start, length);
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

    internal static PooledArray<T> FromCollection(ICollection<T> values, ArrayPool<T> arrayPool)
    {
        PooledArray<T> array = new(arrayPool, values.Count);
        values.CopyTo(array.Array, 0);
        return array;
    }

    internal static PooledArray<T> FromSpan(ReadOnlySpan<T> values, ArrayPool<T> arrayPool)
    {
        PooledArray<T> array = new(arrayPool, values.Length);
        values.CopyTo(array.Array);
        return array;
    }
}