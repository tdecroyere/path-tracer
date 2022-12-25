namespace PathTracer.Core.UnitTests;

public readonly record struct TestParameter
{
    public int Value { get; init; }
}

public class RendererTests
{
    private readonly IRenderer<IImage, TestParameter> _sut;
    private readonly IImage _mockImage;
    private readonly IImageWriter<IImage, TestParameter> _mockImageWriter;
    private readonly Camera _camera;
    private readonly Scene _scene;

    public RendererTests()
    {
        _mockImage = Substitute.For<IImage>();
        _mockImageWriter = Substitute.For<IImageWriter<IImage, TestParameter>>();
        _camera = new Camera();
        _scene = new Scene();

        _sut = new Renderer<IImage, TestParameter>(_mockImageWriter);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    public void Render_ShouldThrowArgumentException_WhenImageSizeIsZero(int imageWidth, int imageHeight)
    {
        // Arrange
        _mockImage.Width.Returns(imageWidth);
        _mockImage.Height.Returns(imageHeight);

        // Act
        var action = () => { _sut.Render(_mockImage, _scene, _camera); };

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void Render_ShouldWriteEveryPixels_WhenDataIsValid()
    {
        // Arrange
        _mockImage.Width.Returns(100);
        _mockImage.Height.Returns(100);

        // Act
        _sut.Render(_mockImage, _scene, _camera);

        // Assert
        _mockImageWriter.Received(100 * 100).StorePixel(_mockImage, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<Vector4>());
    }

    [Fact]
    public void Render_ShouldCallImageWriterCommitImage_WhenCommitImageIsCalled()
    {
        // Arrange
        var testParameter = new TestParameter { Value = 10 };

        // Act
        _sut.CommitImage(_mockImage, testParameter);

        // Act
        _mockImageWriter.Received().CommitImage(_mockImage, testParameter);
    }
    
    [Fact]
    public void Render_ShouldHaveEmptyImage_WhenNothingIsVisible()
    {
        // Arrange
        _mockImage.Width.Returns(100);
        _mockImage.Height.Returns(100);

        // Act
        _sut.Render(_mockImage, _scene, _camera with { Position = Vector3.Zero });

        // Assert
        _mockImageWriter.Received(100 * 100).StorePixel(_mockImage, Arg.Any<int>(), Arg.Any<int>(), new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
    }
}