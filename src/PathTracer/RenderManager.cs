namespace PathTracer;

public class RenderManager
{
    private const float _lowResolutionScaleRatio = 0.25f;
    
    private readonly IGraphicsService _graphicsService;
    private readonly IRenderer<TextureImage> _renderer;
    private readonly IRenderer<FileImage> _fileRenderer;

    private Camera _camera;
    private RenderStatistics _renderStatistics;
    private Task? _fileRenderingTask;

    public void RenderToImage(RenderCommand renderCommand)
    {
        if (_fileRenderingTask == null || _fileRenderingTask.IsCompleted)
        {
            _fileRenderingTask = new Task(() => 
            {
                _renderStatistics.IsFileRenderingActive = true;

                var width = renderCommand.RenderSettings.Resolution.Width;
                var height = renderCommand.RenderSettings.Resolution.Height;

                var outputImage = new FileImage
                {
                    Width = width,
                    Height = height,
                    OutputPath = renderCommand.RenderSettings.OutputPath,
                    ImageData = new Vector4[width * height]
                };

                var fileCamera = _camera with
                {
                    AspectRatio = (float)width / height
                };

                _fileRenderer.Render(outputImage, fileCamera);
                _fileRenderer.CommitImage(outputImage);
                _renderStatistics.IsFileRenderingActive = false;
            });

            _fileRenderingTask.Start();
        }
    }

    public void RenderScene(Camera camera, 
                             CommandList commandList, 
                             Camera previousCamera, 
                             TextureImage _fullResolutionTextureImage, 
                             TextureImage _textureImage,
                             ref bool _isFullResolutionRenderComplete,
                             ref Task<bool>? _fullResolutionRenderingTask,
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
            _renderer.CommitImage(_textureImage);
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
            _renderer.CommitImage(_fullResolutionTextureImage);
            _graphicsService.SubmitCommandList(commandList);
        }
    }
    
    // TODO: To be converted to an ECS System
    private static Camera UpdateCamera(Camera camera, InputState inputState, float deltaTime)
    {
        var forwardInput = inputState.Keyboard.KeyZ.Value - inputState.Keyboard.KeyS.Value;
        var sideInput = inputState.Keyboard.KeyD.Value - inputState.Keyboard.KeyQ.Value;
        var rotateYInput = inputState.Keyboard.Right.Value - inputState.Keyboard.Left.Value;
        var rotateXInput = inputState.Keyboard.Down.Value - inputState.Keyboard.Up.Value;

        // TODO: No acceleration for the moment
        var movementSpeed = 0.5f;
        var rotationSpeed = 0.5f;

        // TODO: Put right direction vector to the Camera struct
        var forwardDirection = camera.Target - camera.Position;
        var rightDirection = Vector3.Cross(new Vector3(0, 1, 0), forwardDirection);

        var movementVector = rightDirection * sideInput * movementSpeed * deltaTime + forwardDirection * forwardInput * movementSpeed * deltaTime;
        var cameraPosition = camera.Position + movementVector;

        var rotateX = rotateXInput * rotationSpeed * deltaTime;
        var rotateY = rotateYInput * rotationSpeed * deltaTime;

        var quaternionX = Quaternion.CreateFromAxisAngle(rightDirection, rotateX);
        var quaternionY = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), rotateY);

        var rotationQuaternion = Quaternion.Normalize(quaternionX * quaternionY);
        forwardDirection = Vector3.Transform(forwardDirection, rotationQuaternion);

        return camera with
        {
            Position = cameraPosition,
            Target = cameraPosition + forwardDirection
        };
    }

    private void CreateRenderTextures(int width, int height, ref TextureImage _textureImage, ref TextureImage _fullResolutionTextureImage)
    {
        var aspectRatio = (float)width / height;
        var lowResWidth = (int)(width * _lowResolutionScaleRatio);
        var lowResHeight = (int)(lowResWidth / aspectRatio);

        _textureImage = CreateOrUpdateTextureImage(_textureImage, lowResWidth, lowResHeight);
        _fullResolutionTextureImage = CreateOrUpdateTextureImage(_fullResolutionTextureImage, width, height);
    }

    private TextureImage CreateOrUpdateTextureImage(TextureImage textureImage, int width, int height)
    {
        // TODO: Call a delete function

        var cpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Staging, TextureType.Texture2D);
        var gpuTexture = _graphicsService.CreateTexture(_graphicsDevice, width, height, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);

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