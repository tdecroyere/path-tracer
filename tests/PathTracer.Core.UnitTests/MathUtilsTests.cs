namespace PathTracer.Core.UnitTests;

public class MathUtilsTests
{
    [Theory]
    [InlineData(0.0f, 0.0f)]
    [InlineData(1.0f, 0.0175f)]
    [InlineData(45.0f, 0.7854f)]
    [InlineData(180.0f, 3.1416f)]
    public void DegreesToRad_Values_HasCorrectResult(float inputValue, float expectedValue)
    {
        // Act
        var output = MathUtils.DegreesToRad(inputValue);

        // Assert
        Assert.Equal(expectedValue, output, 4);
    }

    [Theory]
    [InlineData(0.0f, 0.0f)]
    [InlineData(1.0f, 57.2958f)]
    [InlineData(3.1416f, 180.0004f)]
    public void RadToDegrees_Values_HasCorrectResult(float inputValue, float expectedValue)
    {
        // Act
        var output = MathUtils.RadToDegrees(inputValue);

        // Assert
        Assert.Equal(expectedValue, output, 4);
    }
    
    [Fact]
    public void CreateLookAtMatrix_WithValidParameters_ShouldCreateCorrectMatrix()
    {
        // Arrange
        var cameraPosition = new Vector3(3.0f, 15.0f, -6.5f);
        var cameraTarget = new Vector3(1.0f, 0.5f, 7.8f);
        var cameraUpVector = new Vector3(0.0f, 1.0f, 0.0f);

        // Act
        var result = MathUtils.CreateLookAtMatrix(cameraPosition, cameraTarget, cameraUpVector);

        // Assert
        var zAxis = Vector3.Normalize(cameraTarget - cameraPosition);
        var xAxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zAxis));
        var yAxis = Vector3.Normalize(Vector3.Cross(zAxis, xAxis));

        Assert.Equal(xAxis.X, result[0, 0]);
        Assert.Equal(yAxis.X, result[0, 1]);
        Assert.Equal(zAxis.X, result[0, 2]);
        Assert.Equal(0.0f, result[0, 3]); 
        
        Assert.Equal(xAxis.Y, result[1, 0]);
        Assert.Equal(yAxis.Y, result[1, 1]);
        Assert.Equal(zAxis.Y, result[1, 2]);
        Assert.Equal(0.0f, result[1, 3]); 

        Assert.Equal(xAxis.Z, result[2, 0]);
        Assert.Equal(yAxis.Z, result[2, 1]);
        Assert.Equal(zAxis.Z, result[2, 2]);
        Assert.Equal(0.0f, result[2, 3]); 
        
        Assert.Equal(-Vector3.Dot(xAxis, cameraPosition), result[3, 0]);
        Assert.Equal(-Vector3.Dot(yAxis, cameraPosition), result[3, 1]);
        Assert.Equal(-Vector3.Dot(zAxis, cameraPosition), result[3, 2]);
        Assert.Equal(1.0f, result[3, 3]); 
    }

    [Fact]
    public void CreatePerspectiveFieldOfViewMatrix_WithValidParameters_ShouldCreateCorrectMatrix()
    {
        // Arrange
        var fieldOfViewY = 0.78f;
        var aspectRatio = 1.3f;
        var nearPlaneDistance = 0.01f;

        // Act
        var result = MathUtils.CreatePerspectiveFieldOfViewMatrix(fieldOfViewY, aspectRatio, nearPlaneDistance);

        // Assert
        var height = 1.0f / MathF.Tan(fieldOfViewY / 2.0f);

        Assert.Equal(height / aspectRatio, result[0, 0]);
        Assert.Equal(0.0f, result[0, 1]);
        Assert.Equal(0.0f, result[0, 2]);
        Assert.Equal(0.0f, result[0, 3]);

        Assert.Equal(0.0f, result[1, 0]);
        Assert.Equal(height, result[1, 1]);
        Assert.Equal(0.0f, result[1, 2]);
        Assert.Equal(0.0f, result[1, 3]);

        Assert.Equal(0.0f, result[2, 0]);
        Assert.Equal(0.0f, result[2, 1]);
        Assert.Equal(0.0f, result[2, 2]);
        Assert.Equal(1.0f, result[2, 3]);
        
        Assert.Equal(0.0f, result[3, 0]);
        Assert.Equal(0.0f, result[3, 1]);
        Assert.Equal(nearPlaneDistance, result[3, 2]);
        Assert.Equal(0.0f, result[3, 3]);
    }
}