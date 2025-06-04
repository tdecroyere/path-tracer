namespace PathTracer.IntegrationTests;

public class RenderManagerTests
{
    private readonly IRenderManager _sut;
    private readonly IGraphicsService _mockGraphicsService;
    private readonly IRenderer<TextureImage, CommandList> _mockTextureRenderer;
    private readonly IRenderer<FileImage, string> _mockFileRenderer;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly CommandList _commandList;

    public RenderManagerTests()
    {
        _mockGraphicsService = Substitute.For<IGraphicsService>();
        _mockTextureRenderer = Substitute.For<IRenderer<TextureImage, CommandList>>();
        _mockFileRenderer = Substitute.For<IRenderer<FileImage, string>>();

        _graphicsDevice = nint.Zero;
        _commandList = nint.Zero;

        _sut = new RenderManager(_mockGraphicsService, _mockTextureRenderer, _mockFileRenderer);
    }

    [Fact]
    public void CreateRenderTextures_ShouldCreateTexturesWithCorrectSize_WhenParametersAreValid()
    {
        // Arrange
        const float lowResolutionRatio = 0.25f;
        var renderWidth = 1280;
        var renderHeight = 720;

        var lowResolutionWidth = (int)(renderWidth * lowResolutionRatio);
        var lowResolutionHeight = (int) (renderHeight * lowResolutionRatio);

        // Act
        _sut.CreateRenderTextures(_graphicsDevice, renderWidth, renderHeight);

        // Assert
        _mockGraphicsService.Received().CreateTexture(_graphicsDevice, renderWidth, renderHeight, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);
        _mockGraphicsService.Received().CreateTexture(_graphicsDevice, lowResolutionWidth, lowResolutionHeight, 1, 1, 1, TextureFormat.Rgba8UnormSrgb, TextureUsage.Sampled, TextureType.Texture2D);
    }

    [Fact]
    public void Render_ShouldRenderLowResolutionTexture_WhenFirstRendering()
    {
        // Arrange
        CreateRenderTextures();
        var scene = CreateScene();
        var camera = new Camera();

        // Act
        _sut.RenderScene(_commandList, scene, camera);

        // Assert
        _mockTextureRenderer.Received().Render(new TextureImage(), scene, camera);
    }

    private void CreateRenderTextures()
    {
        var renderWidth = 1280;
        var renderHeight = 720;

        _sut.CreateRenderTextures(_graphicsDevice, renderWidth, renderHeight);
    }

    private static Scene CreateScene()
    {
        return new Scene();
    }
}