namespace PathTracer.Core;

public static class MatrixUtils
{
    public static Matrix4x4 CreatePerspectiveFieldOfViewMatrix(float verticalFov, float aspectRatio, float nearPlaneDistance)
    {
        var height = 1.0f / MathF.Tan(verticalFov / 2.0f);

        var row1 = new Vector4(height / aspectRatio, 0, 0, 0);
        var row2 = new Vector4(0, height, 0, 0);
        var row3 = new Vector4(0, 0, 0, 1);
        var row4 = new Vector4(0, 0, nearPlaneDistance, 0);

        return new Matrix4x4(row1.X, row1.Y, row1.Z, row1.W,
                             row2.X, row2.Y, row2.Z, row2.W,
                             row3.X, row3.Y, row3.Z, row3.W,
                             row4.X, row4.Y, row4.Z, row4.W);
    }
}