using PathTracer.Platform.NativeUI;

namespace PathTracer.Platform;

// TODO: Replace that with Dependency injection from Microsoft package
public class PlatformFactory
{
    private readonly IApplicationService _applicationService;
    private readonly INativeUIService _nativeUIService;

    public PlatformFactory()
    {
        _applicationService = new ApplicationService();
        _nativeUIService = new NativeUIService();
    }

    public IApplicationService GetApplicationService()
    {
        return _applicationService;
    }
    
    public INativeUIService GetNativeUIService()
    {
        return _nativeUIService;
    }
}