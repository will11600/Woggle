# Woggle

[![NuGet version](https://img.shields.io/nuget/v/Woggle?style=flat-square)](https://www.nuget.org/packages/Woggle)
[![License](https://img.shields.io/github/license/will11600/Woggle?style=flat-square)](LICENSE.txt)

Woggle is a .NET library that provides high-performance, memory-efficient collections using `ArrayPool<T>` to reduce garbage collection pressure and improve the performance of your applications.

## Key Features

* **Reduce Memory Allocation by up to 99%:** Woggle's pooled collections can slash memory allocations by up to 99% compared to standard .NET collections, significantly reducing pressure on the garbage collector.
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

## Benchmarks

These benchmarks were run on Windows 11 using a AMD Ryzen 7 5700G processor at 3.80GHz. Package version 2.0.3.

### Pooled List

| Method             | N    | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------- |----- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **List_Add**           | **100**  |   **189.1 ns** |  **3.32 ns** |  **3.11 ns** |  **1.00** |    **0.02** | **0.1414** | **0.0002** |    **1184 B** |        **1.00** |
| PooledList_Add     | 100  |   174.4 ns |  1.51 ns |  1.41 ns |  0.92 |    0.02 | 0.0048 |      - |      40 B |        0.03 |
| List_Iterate       | 100  |   143.4 ns |  1.98 ns |  1.86 ns |  0.76 |    0.02 | 0.0544 |      - |     456 B |        0.39 |
| PooledList_Iterate | 100  |   266.7 ns |  1.22 ns |  1.08 ns |  1.41 |    0.02 | 0.0095 |      - |      80 B |        0.07 |
|                    |      |            |          |          |       |         |        |        |           |             |
| **List_Add**           | **1000** | **1,169.2 ns** | **22.66 ns** | **20.09 ns** |  **1.00** |    **0.02** | **1.0052** |      **-** |    **8424 B** |       **1.000** |
| PooledList_Add     | 1000 | 1,116.7 ns |  2.36 ns |  2.21 ns |  0.96 |    0.02 | 0.0038 |      - |      40 B |       0.005 |
| List_Iterate       | 1000 | 1,328.3 ns | 13.50 ns | 12.62 ns |  1.14 |    0.02 | 0.4845 |      - |    4056 B |       0.481 |
| PooledList_Iterate | 1000 | 2,243.0 ns |  7.08 ns |  6.28 ns |  1.92 |    0.03 | 0.0076 |      - |      80 B |       0.009 |

### Pooled Array

| Method                    | N    | Mean      | Error    | StdDev    | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |----- |----------:|---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Array_CreateAndFill**       | **100**  |  **36.44 ns** | **0.630 ns** |  **0.589 ns** |  **36.65 ns** |  **1.00** |    **0.02** | **0.0507** |     **424 B** |        **1.00** |
| PooledArray_CreateAndFill | 100  |  82.91 ns | 0.372 ns |  0.348 ns |  82.88 ns |  2.28 |    0.04 | 0.0048 |      40 B |        0.09 |
|                           |      |           |          |           |           |       |         |        |           |             |
| **Array_CreateAndFill**       | **1000** | **329.01 ns** | **6.483 ns** | **17.305 ns** | **320.91 ns** |  **1.00** |    **0.07** | **0.4807** |    **4024 B** |       **1.000** |
| PooledArray_CreateAndFill | 1000 | 501.94 ns | 1.015 ns |  0.848 ns | 501.82 ns |  1.53 |    0.07 | 0.0048 |      40 B |       0.010 |