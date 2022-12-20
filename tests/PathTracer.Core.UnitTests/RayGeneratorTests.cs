namespace PathTracer.Core.UnitTests;

public class RayGeneratorTests
{
    private readonly Camera _camera;
    private readonly RayGenerator _sut;

    public RayGeneratorTests()
    {
        _camera = new Camera();
        _sut = new RayGenerator(_camera);
    }

    [Theory]
    [MemberData(nameof(GenerateRay_ShouldThrowArgumentOutOfRangeException_WhenPixelCoordinatesAreInvalid_TestData))]
    public void GenerateRay_ShouldThrowArgumentOutOfRangeException_WhenPixelCoordinatesAreInvalid(Vector2 pixelCoordinates)
    {
        // Act
        var action = () => { _sut.GenerateRay(pixelCoordinates); };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Theory]
    [MemberData(nameof(GenerateRay_ShouldGenerateCorrectRay_WhenPixelCoordinatesAreValid_TestData))]
    public void GenerateRay_ShouldGenerateCorrectRay_WhenPixelCoordinatesAreValid(Vector2 pixelCoordinates)
    {
        // Arrange
        Matrix4x4.Invert(MathUtils.CreateLookAtMatrix(new Vector3(0.0f, 0.0f, -3.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f)), out var inverseViewMatrix);
        Matrix4x4.Invert(MathUtils.CreatePerspectiveFieldOfViewMatrix(MathUtils.DegreesToRad(45.0f), 1.0f, 0.1f), out var inverseProjectionMatrix);
        
        var target = Vector4.Transform(new Vector4(pixelCoordinates.X, pixelCoordinates.Y, 1.0f, 1.0f), inverseProjectionMatrix);
        var rayDirection = Vector4.Transform(new Vector4(Vector3.Normalize(new Vector3(target.X, target.Y, target.Z) / target.W), 0.0f), inverseViewMatrix);

        var expected = new Ray
        {
            Origin = _camera.Position,
            Direction = new Vector3(rayDirection.X, rayDirection.Y, rayDirection.Z)
        };

        // Act
        var ray = _sut.GenerateRay(pixelCoordinates);

        // Assert
        Assert.Equal(expected, ray);
    }

    private static IEnumerable<object[]> GenerateRay_ShouldThrowArgumentOutOfRangeException_WhenPixelCoordinatesAreInvalid_TestData()
    {
        yield return new object[] { new Vector2(-2.0f, 0.0f) };
        yield return new object[] { new Vector2(0.0f, -2.0f) };
        yield return new object[] { new Vector2(2.0f, 0.0f) };
        yield return new object[] { new Vector2(0.0f, 2.0f) };
    }
    
    private static IEnumerable<object[]> GenerateRay_ShouldGenerateCorrectRay_WhenPixelCoordinatesAreValid_TestData()
    {
        yield return new object[] { new Vector2(0.0f, 0.0f) };
        yield return new object[] { new Vector2(1.0f, 1.0f) };
        yield return new object[] { new Vector2(0.0f, 1.0f) };
    }
}