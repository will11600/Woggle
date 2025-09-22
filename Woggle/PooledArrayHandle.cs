using System.Buffers;
using System.Runtime.CompilerServices;

namespace Woggle;

internal sealed class PooledArrayHandle<T> : IDisposable
{
    public ArrayPool<T> Pool { get; }

    private T[]? rentedArray;
    private ArrayHandleFlags _flags;

    public bool Disposed
    {
        get => (_flags & ArrayHandleFlags.Disposed) != 0;
        private set => _flags = value ? _flags | ArrayHandleFlags.Disposed : _flags & ~ArrayHandleFlags.Disposed;
    }

    public bool TypeIsReferenceOrContainsReferences => (_flags & ArrayHandleFlags.TypeIsReferenceOrContainsReferences) != 0;

    public T[] Array
    {
        get
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            return rentedArray!;
        }
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);

            if (ReferenceEquals(rentedArray, value))
            {
                return;
            }

            Pool.Return(rentedArray!, TypeIsReferenceOrContainsReferences);
            rentedArray = value;
        }
    }

    public PooledArrayHandle(int capacity, ArrayPool<T> pool)
    {
        _flags = RuntimeHelpers.IsReferenceOrContainsReferences<T>() ? ArrayHandleFlags.TypeIsReferenceOrContainsReferences : ArrayHandleFlags.None;
        Pool = pool ?? throw new ArgumentNullException(nameof(pool));
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));
        rentedArray = Pool.Rent(capacity);
    }

    ~PooledArrayHandle()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            // This is the place to clean up other managed IDisposable objects if there were any.
        }

        if (rentedArray is not null)
        {
            Pool.Return(rentedArray, TypeIsReferenceOrContainsReferences);
            rentedArray = null;
        }

        Disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}