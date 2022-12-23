using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace PathTracer.Core.PerformanceTests;
/*
public class EmptyImageStorage : IImageStorage
{
    public Task WriteDataAsync(string key, ReadOnlyMemory<byte> data)
    {
        return Task.CompletedTask;
    }
}

public class PpmImageWriterPerformanceTests
{
    private readonly PpmImageWriter imageWriterBase;
    private readonly Vector3[] testData;

    public PpmImageWriterPerformanceTests()
    {
        imageWriterBase = new PpmImageWriter(new EmptyImageStorage());
        testData = new Vector3[1024 * 1024];

        var random = new Random();

        for (var i = 0; i < testData.Length; i++)
        {
            testData[i] = new Vector3(random.NextSingle(), random.NextSingle(), random.NextSingle());
        }
    }

    [Benchmark]
    public async Task WriteImageAsync()
    {
        await imageWriterBase.WriteImageAsync("Test", 1024, 1024, testData);
    }
}*/