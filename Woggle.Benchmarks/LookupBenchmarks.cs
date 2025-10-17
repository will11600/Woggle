using BenchmarkDotNet.Attributes;

namespace Woggle.Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class LookupBenchmarks
{
    // A struct that is Equatable but not Comparable to test the linear search path.
    public readonly struct NonComparableStruct : IEquatable<NonComparableStruct>
    {
        public int Value { get; }

        public NonComparableStruct(int value)
        {
            Value = value;
        }

        public bool Equals(NonComparableStruct other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is NonComparableStruct other && Equals(other);
        public override int GetHashCode() => Value;
    }

    [Params(100, 1000, 10000)]
    public int N;

    // Collections for IComparable type (int)
    private List<int> _listInt = null!;
    private PooledList<int> _pooledListInt = null!;
    private int[] _arrayInt = null!;
    private PooledArray<int> _pooledArrayInt = null!;
    private int _itemToFindInt;

    // Collections for Non-IComparable type
    private List<NonComparableStruct> _listStruct = null!;
    private PooledList<NonComparableStruct> _pooledListStruct = null!;
    private NonComparableStruct _itemToFindStruct;

    [GlobalSetup]
    public void Setup()
    {
        // --- Setup for IComparable<int> ---
        var dataInt = Enumerable.Range(0, N).ToArray();
        _itemToFindInt = N / 2; // Find an item in the middle

        _listInt = new List<int>(dataInt);
        _pooledListInt = dataInt.ToPooledList();
        _arrayInt = dataInt;
        _pooledArrayInt = dataInt.ToPooledArray();


        // --- Setup for NonComparableStruct ---
        var dataStruct = Enumerable.Range(0, N).Select(i => new NonComparableStruct(i)).ToArray();
        _itemToFindStruct = new NonComparableStruct(N / 2);

        _listStruct = new List<NonComparableStruct>(dataStruct);
        _pooledListStruct = dataStruct.ToPooledList();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _pooledListInt.Dispose();
        _pooledArrayInt.Dispose();
        _pooledListStruct.Dispose();
    }

    // --- Benchmarks for IComparable types (should use Binary Search) ---

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("List", "Comparable")]
    public int List_IndexOf()
    {
        // List<T>.IndexOf performs a linear search.
        return _listInt.IndexOf(_itemToFindInt);
    }

    [Benchmark]
    [BenchmarkCategory("List", "Comparable")]
    public int PooledList_IndexOf()
    {
        // PooledList<T>.IndexOf performs a binary search for IComparable types.
        return _pooledListInt.IndexOf(_itemToFindInt);
    }

    [Benchmark]
    [BenchmarkCategory("Array", "Comparable")]
    public int Array_IndexOf()
    {
        // Array.IndexOf performs a linear search.
        return Array.IndexOf(_arrayInt, _itemToFindInt);
    }

    [Benchmark]
    [BenchmarkCategory("Array", "Comparable")]
    public int Array_BinarySearch()
    {
        // Native binary search for comparison.
        return Array.BinarySearch(_arrayInt, _itemToFindInt);
    }

    [Benchmark]
    [BenchmarkCategory("Array", "Comparable")]
    public int PooledArray_IndexOf()
    {
        // PooledArray<T>.IndexOf performs a binary search for IComparable types.
        return _pooledArrayInt.IndexOf(_itemToFindInt);
    }

    // --- Benchmarks for Non-IComparable types (should use Linear Search) ---

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("List", "NonComparable")]
    public int List_IndexOf_NonComparable()
    {
        return _listStruct.IndexOf(_itemToFindStruct);
    }

    [Benchmark]
    [BenchmarkCategory("List", "NonComparable")]
    public int PooledList_IndexOf_NonComparable()
    {
        return _pooledListStruct.IndexOf(_itemToFindStruct);
    }
}
