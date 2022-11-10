using System.Runtime.InteropServices;
using PathTracer.Platform.NativeUI;

namespace PathTracer;

public class PlatformImageWriter : IImageWriter<PlatformImage>
{
    private readonly INativeUIService _nativeUIService;

    public PlatformImageWriter(INativeUIService nativeUIService)
    {
        _nativeUIService = nativeUIService;
    }

 

    public void StorePixel(PlatformImage image, int x, int y, Vector4 pixel)
    {
        var nativeImageInfo = image.NativeSufaceInfo;
        var pixelRowIndex = (image.Height - 1 - y) * image.Width;

        // TODO: Implement Gamma Correction
        pixel *= 255.0f;

        image.ImageData.Span[pixelRowIndex + x] = (uint)pixel.W << nativeImageInfo.AlphaShift | (uint)pixel.Z << nativeImageInfo.BlueShift | (uint)pixel.Y << nativeImageInfo.GreenShift | (uint)pixel.X << nativeImageInfo.RedShift;
    }

    public void CommitImage(PlatformImage image)
    {
        _nativeUIService.UpdateImageSurface(image.NativeSurface, MemoryMarshal.Cast<uint, byte>(image.ImageData.Span));
    }
}