namespace PathTracer.Core;

public class RayGenerator
{
    private readonly Camera _camera;
    private readonly Matrix4x4 _viewMatrix;
    private readonly Matrix4x4 _inverseViewMatrix;
    private readonly Matrix4x4 _projectionMatrix;
    private readonly Matrix4x4 _inverseProjectionMatrix;

    public RayGenerator(Camera camera)
    {
        _camera = camera;

        _viewMatrix = MathUtils.CreateLookAtMatrix(camera.Position, camera.Target, new Vector3(0.0f, 1.0f, 0.0f));
        Matrix4x4.Invert(_viewMatrix, out _inverseViewMatrix);

        _projectionMatrix = MathUtils.CreatePerspectiveFieldOfViewMatrix(MathUtils.DegreesToRad(_camera.VerticalFov), _camera.AspectRatio, _camera.NearPlaneDistance);
        Matrix4x4.Invert(_projectionMatrix, out _inverseProjectionMatrix);
    }
    
    public Ray GenerateRay(Vector2 pixelCoordinates)
    {
        if (pixelCoordinates.X < -1.0f || pixelCoordinates.Y < -1.0f || pixelCoordinates.X > 1.0f || pixelCoordinates.Y > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(pixelCoordinates), "Pixel coordinates should be in the [-1, -1] [1, 1] range.");
        }

        var target = Vector4.Transform(new Vector4(pixelCoordinates.X, pixelCoordinates.Y, 1.0f, 1.0f), _inverseProjectionMatrix);
        var rayDirection = Vector4.Transform(new Vector4(Vector3.Normalize(new Vector3(target.X, target.Y, target.Z) / target.W), 0.0f), _inverseViewMatrix);

        return new Ray
        {
            Origin = _camera.Position,
            Direction = new Vector3(rayDirection.X, rayDirection.Y, rayDirection.Z)
        };
    }
}