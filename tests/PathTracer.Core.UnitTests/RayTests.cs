namespace PathTracer.Core.UnitTests;

public class RayTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDirectionIsZero()
    {
        // Arrange / Act
        var action = () => { new Ray { Origin = Vector3.Zero, Direction = Vector3.Zero }; };

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
    
    [Theory]
    [MemberData(nameof(GetPoint_TestData))]
    public void GetPoint_ShouldReturnCorrectResult_WhenParametersAreValid(Vector3 expected, Vector3 origin, Vector3 direction, float t)
    {
        // Arrange
        var ray = new Ray { Origin = origin, Direction = direction };

        // Act
        var result = ray.GetPoint(t);

        // Assert
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> GetPoint_TestData()
    {
        yield return new object[] { new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), 1.0f };
        yield return new object[] { new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), 0.5f };
        yield return new object[] { new Vector3(4.0f, 4.0f, 4.0f), new Vector3(2.0f, 2.0f, 2.0f), new Vector3(1.0f, 1.0f, 1.0f), 2.0f };
    }
}