namespace PathTracer.Core.UnitTests;

public class CameraTests
{
    // private readonly Camera _sut;

    [Fact]
    // Constructor_ShouldAssignDefaultValues_WhenEmptyParameters
    public void Constructor_WithDefault_SetDefaultValues()
    {
        // Arrange / Act
        var camera = new Camera();

        // Assert
        Assert.Equal(new Vector3(0.0f, 0.0f, -3.0f), camera.Position);
        Assert.Equal(new Vector3(0.0f, 0.0f, 1.0f), camera.Target);
        Assert.Equal(45.0f, camera.VerticalFov);
        Assert.Equal(0.1f, camera.NearPlaneDistance);
        Assert.Equal(1.0f, camera.AspectRatio);
    }
}