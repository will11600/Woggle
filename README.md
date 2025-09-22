# Woggle

Woggle is a .NET library that provides high-performance, memory-efficient collections using `ArrayPool<T>` to reduce garbage collection pressure and improve the performance of your applications.

This library is ideal for scenarios where you are working with large collections of data or in performance-critical code paths where minimizing memory allocations is crucial.

## Key Features

  - **`PooledArray<T>`**: A struct-based, read-only list that is backed by a rented array from an `ArrayPool<T>`.
  - **`PooledList<T>`**: A mutable list, similar to `List<T>`, but with its underlying array rented from an `ArrayPool<T>`.
  - **`CollectionExtensions`**: A set of extension methods for easily converting existing collections (`ICollection<T>`, `T[]`, `ReadOnlySpan<T>`) into `PooledArray<T>` and `PooledList<T>`.
  - **Memory Efficiency**: By using `ArrayPool<T>`, Woggle helps you avoid frequent and large memory allocations, leading to fewer garbage collections and a more stable application performance.

## Getting Started

To get started with the Woggle library, you can include the source files in your project.

### Using `PooledList<T>`

The `PooledList<T>` is a versatile, mutable list that's perfect for when you need a dynamic collection but want to avoid the memory overhead of a standard `List<T>`.

Here is how you can use `PooledList<T>` in your project:

```csharp
using Woggle;
using System;
using System.Buffers;

// Create a new PooledList with a default capacity
using var pooledList = new PooledList<int>();

// Add items to the list
pooledList.Add(10);
pooledList.Add(20);
pooledList.Add(30);

// Access elements by index
Console.WriteLine(pooledList[1]); // Output: 20

// The underlying array is automatically returned to the pool when disposed
```

### Using `PooledArray<T>`

If you need an static collection, `PooledArray<T>` is an excellent choice.

```csharp
using Woggle;
using System;
using System.Buffers;

// Create a PooledArray from a ReadOnlySpan
ReadOnlySpan<int> sourceData = new int[] { 1, 2, 3, 4, 5 };
using var pooledArray = new PooledArray<int>(sourceData);

// Access elements
Console.WriteLine(pooledArray[2]); // Output: 3

// The array is returned to the pool upon disposal
```

## Effortless Conversion with Extension Methods

Woggle's `CollectionExtensions` make it incredibly easy to convert your existing collections into pooled collections.

```csharp
using Woggle;
using System;
using System.Collections.Generic;

// Convert a standard List<T> to a PooledList<T>
var originalList = new List<int> { 100, 200, 300 };
using var pooledListFromList = originalList.ToPooledList();

Console.WriteLine(pooledListFromList.Count); // Output: 3

// Convert an array to a PooledArray<T>
var originalArray = new[] { 5, 10, 15, 20 };
using var pooledArrayFromArray = originalArray.ToPooledArray();

Console.WriteLine(pooledArrayFromArray[1]); // Output: 10
```

## Working with `Span<T>` for High-Performance Scenarios

Both `PooledArray<T>` can be implicitly converted to `Span<T>` and `ReadOnlySpan<T>`, enabling you to work with them in high-performance, allocation-free APIs.

```csharp
using Woggle;
using System;

void ProcessData(Span<int> data)
{
    for (int i = 0; i < data.Length; i++)
    {
        data[i] *= 2;
    }
}

using var list = new PooledList<int> { 1, 2, 3 };

// Implicitly convert PooledList<T> to Span<T>
ProcessData(list.AsSpan());

foreach (var item in list)
{
    Console.WriteLine(item); // Output: 2, 4, 6
}
```

## Custom `ArrayPool<T>` Support

For advanced scenarios, you can provide your own instance of `ArrayPool<T>` to have more control over the memory pooling behavior.

```csharp
using Woggle;
using System;
using System.Buffers;

// Create a custom ArrayPool
var customPool = ArrayPool<int>.Create();

// Use the custom pool with PooledList
using var pooledListWithCustomPool = new PooledList<int>(customPool);
pooledListWithCustomPool.Add(15);

// Use the custom pool with the extension methods
var data = new[] { 1, 2, 3 };
using var pooledArrayWithCustomPool = data.ToPooledArray(customPool);
```
