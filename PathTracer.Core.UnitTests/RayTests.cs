namespace PathTracer.Core.UnitTests;

public class RayTests
{
    [Fact]
    public void Constructor_WithZeroDirection_ThrowsArgumentException()
    {
        // Arrange / Act
        var action = () => { new Ray(Vector3.Zero, Vector3.Zero); };

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [MemberData(nameof(GetPointTestData))]
    public void GetPoint_WithValue_ReturnCorrectResult(Vector3 expected, Vector3 origin, Vector3 direction, float t)
    {
        // Arrange
        var ray = new Ray(origin, direction);

        // Act
        var result = ray.GetPoint(t);

        // Assert
        Assert.Equal(expected, result);
    }

    private static IEnumerable<object[]> GetPointTestData()
    {
        yield return new object[] { new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), 1.0f };
        yield return new object[] { new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), 0.5f };
    }
}