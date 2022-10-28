namespace PathTracer.Core.UnitTests;

public class MatrixUtilsTests
{
    [Fact]
    public void CreatePerspectiveFieldOfViewMatrix_WithValidParameters_ShouldCreateCorrectMatrix()
    {
        // Arrange
        var fieldOfViewY = 45.0f;
        var aspectRatio = 1.3f;
        var nearPlaneDistance = 0.01f;

        // Act
        var result = MatrixUtils.CreatePerspectiveFieldOfViewMatrix(fieldOfViewY, aspectRatio, nearPlaneDistance);

        // Assert
        var height = 1.0f / MathF.Tan(fieldOfViewY / 2.0f);

        Assert.Equal(height / aspectRatio, result[0, 0]);
        Assert.Equal(height, result[1, 1]);
        Assert.Equal(0.0f, result[2, 2]);
        Assert.Equal(1.0f, result[2, 3]);
        Assert.Equal(0.0f, result[3, 3]);
        Assert.Equal(nearPlaneDistance, result[3, 2]);
    }
}