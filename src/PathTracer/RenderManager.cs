namespace PathTracer;

public class RenderManager
{
    private const float _lowResolutionScaleRatio = 0.25f;
    
    private readonly IGraphicsService _graphicsService;
    private readonly IUIService _uiService;
    private readonly IRenderer<TextureImage, CommandList> _renderer;
    private readonly IRenderer<FileImage, string> _fileRenderer;

    private Task? _fileRenderingTask;
    private bool _isFullResolutionRenderComplete = false;
    
    TextureImage _textureImage;
    TextureImage _fullResolutionTextureImage;
    Task<bool>? _fullResolutionRenderingTask = null;

    public RenderManager(IGraphicsService graphicsService, 
                         IUIService uiService, 
                         IRenderer<TextureImage, CommandList> renderer, 
                         IRenderer<FileImage, string> fileRenderer)
    {
        _graphicsService = graphicsService;
        _uiService = uiService;
        _renderer = renderer;
        _fileRenderer = fileRenderer;
    }

    public TextureImage CurrentTextureImage => _isFullResolutionRenderComplete ? _fullResolutionTextureImage : _textureImage;
    
    public ValueTask<bool> RenderToImage(Camera camera, RenderCommand renderCommand)
    {
        if (_fileRenderingTask == null || _fileRenderingTask.IsCompleted)
        {
            _fileRenderingTask = new Task<bool>(() => 
            {
                var width = renderCommand.RenderSettings.Resolution.Width;
                var height = renderCommand.RenderSettings.Resolution.Height;

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
                _fileRenderer.CommitImage(outputImage, renderCommand.RenderSettings.OutputPath);
                return true;
            });

            _fileRenderingTask.Start();
        }

        return ValueTask.FromResult(true);
    }

    public void RenderScene(Camera camera, 
                             CommandList commandList, 
                             Camera previousCamera, 
                             ref RenderStatistics renderStatistics)
    {
        var renderStopwatch = renderStatistics.RenderStopwatch;
        
        // TODO: Do we need a global task, can we reuse task with a pool?
        if (camera != previousCamera)
        {
            Console.WriteLine("Render Low Resolution");
            renderStopwatch.Restart();
            _renderer.Render(_textureImage, camera);
            renderStopwatch.Stop();
            _graphicsService.ResetCommandList(commandList);
            _renderer.CommitImage(_textureImage, commandList);
            _graphicsService.SubmitCommandList(commandList);

            // TODO: Cancel task when possible
            _isFullResolutionRenderComplete = false;
            _fullResolutionRenderingTask = null;
        }
        else if (_fullResolutionRenderingTask == null && _isFullResolutionRenderComplete == false)
        {
            _fullResolutionRenderingTask = new Task<bool>(() =>
            {
                Console.WriteLine("Render Full Resolution");
                renderStopwatch.Restart();
                _renderer.Render(_fullResolutionTextureImage, camera);
                renderStopwatch.Stop();
                return true;
            });

            _fullResolutionRenderingTask.Start();
        }

        if (_fullResolutionRenderingTask != null && _fullResolutionRenderingTask.Status == TaskStatus.RanToCompletion)
        {
            _isFullResolutionRenderComplete = _fullResolutionRenderingTask.Result;
            _fullResolutionRenderingTask = null;
            renderStatistics.LastRenderTime = DateTime.Now;

            _graphicsService.ResetCommandList(commandList);
            _renderer.CommitImage(_fullResolutionTextureImage, commandList);
            _graphicsService.SubmitCommandList(commandList);
        }
    }
   
    public void CreateRenderTextures(GraphicsDevice graphicsDevice, int width, int height)
    {
        var aspectRatio = (float)width / height;
        var lowResWidth = (int)(width * _lowResolutionScaleRatio);
        var lowResHeight = (int)(lowResWidth / aspectRatio);

        _textureImage = CreateOrUpdateTextureImage(graphicsDevice, in _textureImage, lowResWidth, lowResHeight);
        _fullResolutionTextureImage = CreateOrUpdateTextureImage(graphicsDevice, in _fullResolutionTextureImage, width, height);
    }

    private TextureImage CreateOrUpdateTextureImage(GraphicsDevice graphicsDevice, in TextureImage textureImage, int width, int height)
    {
        // TODO: Call a delete function

        var cpuTexture = _graphicsService.CreateTexture(graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Staging, TextureType.Texture2D);
        var gpuTexture = _graphicsService.CreateTexture(graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);

        var imageData = new uint[width * height];
        var textureId = textureImage.TextureId;

        if (textureId == 0)
        {
            textureId = _uiService.RegisterTexture(gpuTexture);
        }
        else
        {
            _uiService.UpdateTexture(textureId, gpuTexture);
        }

        return textureImage with
        {
            Width = width,
            Height = height,
            CpuTexture = cpuTexture,
            GpuTexture = gpuTexture,
            ImageData = imageData,
            TextureId = textureId
        };
    }
}