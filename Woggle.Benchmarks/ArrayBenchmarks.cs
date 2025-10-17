using BenchmarkDotNet.Attributes;

namespace Woggle.Benchmarks;

[MemoryDiagnoser]
public class ArrayBenchmarks
{
    [Params(100, 1000)]
    public int N;

    [Benchmark(Baseline = true)]
    public void Array_CreateAndFill()
    {
        var array = new int[N];
        for (int i = 0; i < N; i++)
        {
            array[i] = i;
        }
    }

    [Benchmark]
    public void PooledArray_CreateAndFill()
    {
        using var pooledArray = new PooledArray<int>(N);
        for (int i = 0; i < N; i++)
        {
            pooledArray[i] = i;
        }
    }
}