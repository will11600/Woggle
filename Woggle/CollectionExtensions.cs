using System.Buffers;

namespace Woggle;

/// <summary>
/// Provides extension methods for creating pooled collections from various sources.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Converts an <see cref="ICollection{T}"/> to a <see cref="PooledArray{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The collection to convert.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the collection.</returns>
    public static PooledArray<T> ToPooledArray<T>(this T values) where T : ICollection<T>
    {
        return ToPooledArray(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts an <see cref="ICollection{T}"/> to a <see cref="PooledList{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The collection to convert.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the collection.</returns>
    public static PooledList<T> ToPooledList<T>(this T values) where T : ICollection<T>
    {
        return ToPooledList(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <typeparamref name="T"/> array to a <see cref="PooledArray{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="values">The array to convert.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the array.</returns>
    public static PooledArray<T> ToPooledArray<T>(this T[] values)
    {
        return ToPooledArray(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <typeparamref name="T"/> array to a <see cref="PooledList{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="values">The array to convert.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the array.</returns>
    public static PooledList<T> ToPooledList<T>(this T[] values)
    {
        return ToPooledList(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="PooledArray{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="values">The span to convert.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the span.</returns>
    public static PooledArray<T> ToPooledArray<T>(this ReadOnlySpan<T> values)
    {
        return ToPooledArray(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="PooledList{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="values">The span to convert.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the span.</returns>
    public static PooledList<T> ToPooledList<T>(this ReadOnlySpan<T> values)
    {
        return ToPooledList(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts an <see cref="ICollection{T}"/> to a <see cref="PooledArray{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The collection to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the collection.</returns>
    public static PooledArray<T> ToPooledArray<T>(this T values, ArrayPool<T> arrayPool) where T : ICollection<T>
    {
        int count = values.Count;
        PooledArrayHandle<T> handle = new(count, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledArray<T>(handle, count);
    }

    /// <summary>
    /// Converts an <see cref="ICollection{T}"/> to a <see cref="PooledList{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The collection to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the collection.</returns>
    public static PooledList<T> ToPooledList<T>(this T values, ArrayPool<T> arrayPool) where T : ICollection<T>
    {
        int count = values.Count;
        PooledArrayHandle<T> handle = new(count, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledList<T>(handle, count);
    }

    /// <summary>
    /// Converts a <typeparamref name="T"/> array to a <see cref="PooledArray{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="values">The array to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the array.</returns>
    public static PooledArray<T> ToPooledArray<T>(this T[] values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledArray<T>(handle, length);
    }

    /// <summary>
    /// Converts a <typeparamref name="T"/> array to a <see cref="PooledList{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="values">The array to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the array.</returns>
    public static PooledList<T> ToPooledList<T>(this T[] values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array, 0);
        return new PooledList<T>(handle, length);
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="PooledArray{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="values">The span to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the span.</returns>
    public static PooledArray<T> ToPooledArray<T>(this ReadOnlySpan<T> values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array.AsSpan());
        return new PooledArray<T>(handle, length);
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="PooledList{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="values">The span to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the span.</returns>
    public static PooledList<T> ToPooledList<T>(this ReadOnlySpan<T> values, ArrayPool<T> arrayPool)
    {
        int length = values.Length;
        PooledArrayHandle<T> handle = new(length, arrayPool);
        values.CopyTo(handle.Array.AsSpan());
        return new PooledList<T>(handle, length);
    }
}