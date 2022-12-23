namespace PathTracer;

public class RenderManager : IRenderManager
{
    private const float _lowResolutionScaleRatio = 0.25f;

    private readonly IGraphicsService _graphicsService;
    private readonly IRenderer<TextureImage, CommandList> _renderer;
    private readonly IRenderer<FileImage, string> _fileRenderer;

    private readonly Stopwatch _renderStopwatch;

    private Task? _fileRenderingTask;
    private Task? _fullResolutionRenderingTask = null;
    private bool _isFullResolutionRenderComplete = false;

    private TextureImage _textureImage;
    private TextureImage _fullResolutionTextureImage;
    private Camera _camera;

    public RenderManager(IGraphicsService graphicsService,
                         IRenderer<TextureImage, CommandList> renderer,
                         IRenderer<FileImage, string> fileRenderer)
    {
        _graphicsService = graphicsService;
        _renderer = renderer;
        _fileRenderer = fileRenderer;

        _renderStopwatch = new Stopwatch();
        _camera = new Camera();
    }

    public TextureImage CurrentTextureImage => _isFullResolutionRenderComplete ? _fullResolutionTextureImage : _textureImage;
    public bool IsFileRenderingActive => !_fileRenderingTask?.IsCompleted ?? false;
    public DateTime LastRenderTime { get; private set; }
    public long RenderDuration { get; private set; }

    public void CreateRenderTextures(GraphicsDevice graphicsDevice, int width, int height)
    {
        var aspectRatio = (float)width / height;
        var lowResWidth = (int)(width * _lowResolutionScaleRatio);
        var lowResHeight = (int)(lowResWidth / aspectRatio);

        _textureImage = CreateOrUpdateTextureImage(graphicsDevice, in _textureImage, lowResWidth, lowResHeight);
        _fullResolutionTextureImage = CreateOrUpdateTextureImage(graphicsDevice, in _fullResolutionTextureImage, width, height);
    }

    public void RenderScene(CommandList commandList, Camera camera)
    {
        if (camera != _camera)
        {
            Console.WriteLine("Render Low Resolution");
            _renderStopwatch.Restart();
            _renderer.Render(_textureImage, camera);
            _renderStopwatch.Stop();
            _graphicsService.ResetCommandList(commandList);
            _renderer.CommitImage(_textureImage, commandList);
            _graphicsService.SubmitCommandList(commandList);

            RenderDuration = _renderStopwatch.ElapsedMilliseconds;

            // TODO: Cancel task when possible
            _isFullResolutionRenderComplete = false;
            _fullResolutionRenderingTask = null;
        }
        else if (_fullResolutionRenderingTask == null && _isFullResolutionRenderComplete == false)
        {
            _fullResolutionRenderingTask = new Task(() =>
            {
                Console.WriteLine("Render Full Resolution");
                _renderStopwatch.Restart();
                _renderer.Render(_fullResolutionTextureImage, camera);
                _renderStopwatch.Stop();
                RenderDuration = _renderStopwatch.ElapsedMilliseconds;
            });

            _fullResolutionRenderingTask.Start();
        }

        if (_fullResolutionRenderingTask != null && _fullResolutionRenderingTask.Status == TaskStatus.RanToCompletion)
        {
            _isFullResolutionRenderComplete = true;
            _fullResolutionRenderingTask = null;
            LastRenderTime = DateTime.Now;

            _graphicsService.ResetCommandList(commandList);
            _renderer.CommitImage(_fullResolutionTextureImage, commandList);
            _graphicsService.SubmitCommandList(commandList);
        }

        _camera = camera;
    }

    public void RenderToImage(RenderSettings renderSettings, Camera camera)
    {
        if (_fileRenderingTask == null || _fileRenderingTask.IsCompleted)
        {
            _fileRenderingTask = new Task(() =>
            {
                var width = renderSettings.Resolution.Width;
                var height = renderSettings.Resolution.Height;
                var outputPath = renderSettings.OutputPath;

                var outputImage = new FileImage
                {
                    Width = width,
                    Height = height,
                    ImageData = new Vector4[width * height]
                };

                var fileCamera = camera with
                {
                    AspectRatio = (float)width / height
                };

                _fileRenderer.Render(outputImage, fileCamera);
                _fileRenderer.CommitImage(outputImage, outputPath);
            });

            _fileRenderingTask.Start();
        }
    }

    public void CheckRenderToImageErrors()
    {
        if (_fileRenderingTask != null && _fileRenderingTask.Exception != null)
        {
            Console.WriteLine(_fileRenderingTask.Exception);
            _fileRenderingTask = null;
        }
    }

    private TextureImage CreateOrUpdateTextureImage(GraphicsDevice graphicsDevice, in TextureImage textureImage, int width, int height)
    {
        // TODO: Call a delete function
        var cpuTexture = _graphicsService.CreateTexture(graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Staging, TextureType.Texture2D);
        var gpuTexture = _graphicsService.CreateTexture(graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);

        var imageData = new uint[width * height];

        return textureImage with
        {
            Width = width,
            Height = height,
            CpuTexture = cpuTexture,
            GpuTexture = gpuTexture,
            ImageData = imageData
        };
    }
}