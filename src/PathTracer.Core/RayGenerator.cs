namespace PathTracer.Core;

public class RayGenerator
{
    private readonly Camera _camera;
    private readonly Matrix4x4 _projectionMatrix;
    private readonly Matrix4x4 _inverseProjectionMatrix;

    public RayGenerator(Camera camera)
    {
        _camera = camera;

        _projectionMatrix = MatrixUtils.CreatePerspectiveFieldOfViewMatrix(_camera.VerticalFov, _camera.AspectRatio, _camera.NearPlaneDistance);

        if (!Matrix4x4.Invert(_projectionMatrix, out _inverseProjectionMatrix))
        {
            throw new InvalidOperationException("Cannot invert the projection matrix.");
        }
    }
    
    public Ray GenerateRay(Vector2 pixelCoordinates)
    {
        if (pixelCoordinates.X < -1.0f || pixelCoordinates.Y < -1.0f || pixelCoordinates.X > 1.0f || pixelCoordinates.Y > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(pixelCoordinates), "Pixel coordinates should be in the [-1, -1] [1, 1] range.");
        }

        var target = Vector4.Transform(new Vector4(pixelCoordinates.X, pixelCoordinates.Y, 1.0f, 1.0f), _inverseProjectionMatrix);
        var rayDirection = new Vector3(target.X, target.Y, target.Z) / target.W;

        return new Ray
        {
            Origin = _camera.Position,
            Direction = rayDirection
        };
    }
}