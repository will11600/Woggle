# Woggle

[![NuGet version](https://img.shields.io/nuget/v/Woggle?style=flat-square)](https://www.nuget.org/packages/Woggle)
[![License](https://img.shields.io/github/license/will11600/Woggle?style=flat-square)](LICENSE.txt)

Woggle is a .NET library that provides high-performance, memory-efficient collections using `ArrayPool<T>` to reduce garbage collection pressure and improve the performance of your applications.

## Key Features

* **GC Reduction**: Collections rent and return array buffers to an `ArrayPool<T>`, which minimizes temporary object creation and reduces garbage collector workload.
* **Pooled Collections**: Provides two main high-performance, disposable collection types:
    * **`PooledList<T>`**: A resizable list implementation, similar to `List<T>`, but backed by a rented array that automatically expands capacity as needed.
    * **`PooledArray<T>`**: A fixed-size array wrapper, designed for highly efficient, span-based operations on a pre-allocated array segment.
* **Span Integration**: Both collections are fully integrated with modern .NET primitives, offering explicit conversion operators to `Span<T>` and `ReadOnlySpan<T>` for zero-allocation memory access and manipulation.
* **Convenience Extensions**: Easily convert existing .NET collections, arrays, and spans into pooled variants using extension methods like `ToPooledArray()` and `ToPooledList()`.

## Getting Started

You can install the library through the NuGet Package Manager:

```bash
Install-Package Woggle
```

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

`PooledArray<T>` is a fixed-size structure, and attempts to modify its length (e.g., calling Add or Remove) will throw a NotSupportedException.

```csharp
using Woggle;
using System.Buffers;

// Rents an array of the specified length (5) from the pool.
using (var pooledArray = new PooledArray<string>(5))
{
    pooledArray[0] = "Hello";
    pooledArray[1] = "Woggle";

    // Access the contents as a span
    ReadOnlySpan<string> span = pooledArray.AsSpan();
    Console.WriteLine($"Length: {pooledArray.Length}"); // Output: Length: 5

    // Implicit conversion to Span<T>
    Span<string> arraySpan = pooledArray;
    // arraySpan is a Span<string> over the entire rented array segment.
} // The rented array is automatically returned to the pool here.
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

`PooledArray<T>` can be implicitly converted to `Span<T>` and `ReadOnlySpan<T>`, enabling you to work with them in high-performance, allocation-free APIs.

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
