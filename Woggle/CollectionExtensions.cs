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
    public static PooledArray<T> ToPooledArray<T>(this ICollection<T> values)
    {
        return PooledArray<T>.FromCollection(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts an <see cref="ICollection{T}"/> to a <see cref="PooledList{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The collection to convert.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the collection.</returns>
    public static PooledList<T> ToPooledList<T>(this T values) where T : ICollection<T>
    {
        return PooledList<T>.FromCollection(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <typeparamref name="T"/> array to a <see cref="PooledArray{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="values">The array to convert.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the array.</returns>
    public static PooledArray<T> ToPooledArray<T>(this T[] values)
    {
        return PooledArray<T>.FromCollection(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <typeparamref name="T"/> array to a <see cref="PooledList{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="values">The array to convert.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the array.</returns>
    public static PooledList<T> ToPooledList<T>(this T[] values)
    {
        return PooledList<T>.FromCollection(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="PooledArray{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="values">The span to convert.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the span.</returns>
    public static PooledArray<T> ToPooledArray<T>(this ReadOnlySpan<T> values)
    {
        return PooledArray<T>.FromSpan(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="PooledList{T}"/> using the shared <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="values">The span to convert.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the span.</returns>
    public static PooledList<T> ToPooledList<T>(this ReadOnlySpan<T> values)
    {
        return PooledList<T>.FromSpan(values, ArrayPool<T>.Shared);
    }

    /// <summary>
    /// Converts an <see cref="ICollection{T}"/> to a <see cref="PooledArray{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The collection to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the elements of the collection.</returns>
    public static PooledArray<T> ToPooledArray<T>(this ICollection<T> values, ArrayPool<T> arrayPool)
    {
        return PooledArray<T>.FromCollection(values, arrayPool);
    }

    /// <summary>
    /// Converts an <see cref="ICollection{T}"/> to a <see cref="PooledList{T}"/> using a specified <see cref="ArrayPool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="values">The collection to convert.</param>
    /// <param name="arrayPool">The array pool to use for renting the array.</param>
    /// <returns>A new <see cref="PooledList{T}"/> containing the elements of the collection.</returns>
    public static PooledList<T> ToPooledList<T>(this ICollection<T> values, ArrayPool<T> arrayPool)
    {
        return PooledList<T>.FromCollection(values, arrayPool);
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
        return PooledArray<T>.FromSpan(values, arrayPool);
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
        return PooledList<T>.FromSpan(values, arrayPool);
    }
}