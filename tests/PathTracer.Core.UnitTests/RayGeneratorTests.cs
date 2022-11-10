namespace PathTracer.Core.UnitTests;

public class RayGeneratorTests
{
    // private readonly RayGenerator _sut;

    [Theory]
    [MemberData(nameof(GenerateRay_WithInvalidPixelCoordinates_TestData))]
    // GenerateRay_ShouldThrowArgumentOutOfRangeException_WhenPixelCoordinatesAreInvalid
    public void GenerateRay_WithInvalidPixelCoordinates_ShouldThrowArgumentOutOfRangeException(Vector2 pixelCoordinates)
    {
        // Arrange
        var generator = new RayGenerator(new Camera());

        // Act
        var action = () => { generator.GenerateRay(pixelCoordinates); };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    // TODO: Add Correct tests
    
    private static IEnumerable<object[]> GenerateRay_WithInvalidPixelCoordinates_TestData()
    {
        yield return new object[] { new Vector2(-2.0f, 0.0f) };
        yield return new object[] { new Vector2(0.0f, -2.0f) };
        yield return new object[] { new Vector2(2.0f, 0.0f) };
        yield return new object[] { new Vector2(0.0f, 2.0f) };
    }
}