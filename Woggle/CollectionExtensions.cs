using System.Buffers;

namespace Woggle;

public static class CollectionExtensions
{
    public static PooledArray<T> ToPooledArray<T>(this T values) where T : ICollection<T>
    {
        return ToPooledArray(values, ArrayPool<T>.Shared);
    }

    public static PooledList<T> ToPooledList<T>(this T values) where T : ICollection<T>
    {
        return ToPooledList(values, ArrayPool<T>.Shared);
    }

    public static PooledArray<T> ToPooledArray<T>(this T[] values)
    {
        return ToPooledArray(values, ArrayPool<T>.Shared);
    }

    public static PooledList<T> ToPooledList<T>(this T[] values)
    {
        return ToPooledList(values, ArrayPool<T>.Shared);
    }

    public static PooledArray<T> ToPooledArray<T>(this ReadOnlySpan<T> values)
    {
        return ToPooledArray(values, ArrayPool<T>.Shared);
    }

    public static PooledList<T> ToPooledList<T>(this ReadOnlySpan<T> values)
    {
        return ToPooledList(values, ArrayPool<T>.Shared);
    }

    public static PooledArray<T> ToPooledArray<T>(this T values, ArrayPool<T> arrayPool) where T : ICollection<T>
    {
        int count = values.Count;
        PooledArrayHandle<T> handle = new(count, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledArray<T>(handle, count);
    }

    public static PooledList<T> ToPooledList<T>(this T values, ArrayPool<T> arrayPool) where T : ICollection<T>
    {
        int count = values.Count;
        PooledArrayHandle<T> handle = new(count, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledList<T>(handle, count);
    }

    public static PooledArray<T> ToPooledArray<T>(this T[] values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledArray<T>(handle, length);
    }

    public static PooledList<T> ToPooledList<T>(this T[] values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledList<T>(handle, length);
    }

    public static PooledArray<T> ToPooledArray<T>(this ReadOnlySpan<T> values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array.AsSpan());
        return new PooledArray<T>(handle, length);
    }

    public static PooledList<T> ToPooledList<T>(this ReadOnlySpan<T> values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array.AsSpan());
        return new PooledList<T>(handle, length);
    }
}