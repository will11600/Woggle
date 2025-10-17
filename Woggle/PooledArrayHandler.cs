using System.Buffers;
using System.Runtime.CompilerServices;

namespace Woggle;

/// <summary>
/// Provides a base class for types that handle rented arrays from an <see cref="ArrayPool{T}"/>.
/// This class manages the renting and returning of the underlying array when disposed or when the internal
/// array is replaced. It implements <see cref="IDisposable"/> to ensure the array is returned to the pool.
/// </summary>
/// <typeparam name="T">The type of elements in the pooled array.</typeparam>
public abstract class PooledArrayHandler<T> : IDisposable
{
    /// <summary>
    /// Indicates whether the type <typeparamref name="T"/> is a reference type or contains references.
    /// This value is used when returning the array to the pool to determine if the array should be cleared.
    /// </summary>
    protected static readonly bool t_isReferenceOrContainsReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();

    /// <summary>
    /// Gets the <see cref="ArrayPool{T}"/> instance used to rent and return the array.
    /// </summary>
    /// <value>The <see cref="ArrayPool{T}"/> provided during construction.</value>
    public ArrayPool<T> Pool { get; }

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    /// <value><see langword="true"/> if the object is disposed; otherwise, <see langword="false"/>.</value>
    protected bool Disposed { get; private set; }

    private T[]? _rentedArray;
    /// <summary>
    /// Gets or sets the underlying array rented from the <see cref="Pool"/>.
    /// </summary>
    /// <value>The rented <typeparamref name="T"/> array.</value>
    /// <remarks>
    /// Setting this property automatically returns the previously rented array (if one exists) 
    /// to the <see cref="Pool"/>, optionally clearing it based on <see cref="t_isReferenceOrContainsReferences"/>.
    /// </remarks>
    protected T[] Array
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _rentedArray!;
        set
        {
            if (ReferenceEquals(_rentedArray, value))
            {
                return;
            }

            if (_rentedArray is not null)
            {
                Pool.Return(_rentedArray, t_isReferenceOrContainsReferences);
            }

            _rentedArray = value;
        }
    }

    internal PooledArrayHandler(int capacity, ArrayPool<T> pool)
    {
        Disposed = false;
        Pool = pool ?? throw new ArgumentNullException(nameof(pool));
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));
        _rentedArray = Pool.Rent(capacity);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PooledArrayHandler{T}"/> class.
    /// </summary>
    ~PooledArrayHandler()
    {
        Dispose(false);
    }

    /// <summary>
    /// Enumerates through the contents of the pooled array.
    /// </summary>
    /// <param name="count">The number of items from the start of the array to enumerate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IEnumerator<T> EnumerateContents(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return _rentedArray![i];
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="PooledArrayHandler{T}"/> and 
    /// optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            // This is the place to clean up other managed IDisposable objects if there were any.
        }

        if (_rentedArray is not null)
        {
            Pool.Return(_rentedArray, t_isReferenceOrContainsReferences);
            _rentedArray = null;
        }

        Disposed = true;
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Determines the index of specific item in the <see cref="Array"/>.
    /// </summary>
    /// <param name="count">The number of elements from the start of the array to search.</param>
    /// <param name="item">The <typeparamref name="T"/> to locate in the array.</param>
    /// <returns>The index of <paramref name="item"/> if found in the array; otherwise -1.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int IndexOf(int count, T item)
    {
        for (int i = 0; i < count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(Array[i], item))
            {
                return i;
            }
        }

        return -1;
    }
}