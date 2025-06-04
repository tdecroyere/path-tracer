namespace PathTracer.Core;

public class RandomGenerator : IRandomGenerator
{
    private readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random(GetSeed()));

    public Vector3 GetVector3()
    {
        var randomLocal = _random.Value;

        return new Vector3(randomLocal.NextSingle() - 0.5f, randomLocal.NextSingle() - 0.5f, randomLocal.NextSingle() - 0.5f);
    }

    private static int GetSeed()
    {
        return Environment.TickCount * Environment.CurrentManagedThreadId;
    }
}