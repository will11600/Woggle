using BenchmarkDotNet.Attributes;

namespace Woggle.Benchmarks;

[MemoryDiagnoser]
public class ListBenchmarks
{
    [Params(100, 1000)]
    public int N;

    [Benchmark(Baseline = true)]
    public void List_Add()
    {
        var list = new List<int>();
        for (int i = 0; i < N; i++)
        {
            list.Add(i);
        }
    }

    [Benchmark]
    public void PooledList_Add()
    {
        using var pooledList = new PooledList<int>();
        for (int i = 0; i < N; i++)
        {
            pooledList.Add(i);
        }
    }

    [Benchmark]
    public long List_Iterate()
    {
        var list = new List<int>(N);
        for (int i = 0; i < N; i++)
        {
            list.Add(i);
        }

        long sum = 0;
        foreach (var item in list)
        {
            sum += item;
        }
        return sum;
    }

    [Benchmark]
    public long PooledList_Iterate()
    {
        using var pooledList = new PooledList<int>(N);
        for (int i = 0; i < N; i++)
        {
            pooledList.Add(i);
        }

        long sum = 0;
        foreach (var item in pooledList)
        {
            sum += item;
        }
        return sum;
    }
}
