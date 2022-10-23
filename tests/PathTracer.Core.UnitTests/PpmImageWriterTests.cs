namespace PathTracer.Core.UnitTests;

internal record PpmTestContent
{
    public required string Header { get; init; } 
    public required int ImageWidth { get; init; }
    public required int ImageHeight { get; init; }
    public required int ImageMode { get; init; } 
    public required ReadOnlyMemory<Vector3> Data { get; init; }
}

public class PPmImageWriterTests
{
    [Fact]
    public async Task WriteImage_WithNoData_HasCorrectHeader()
    {
        // Arrange
        var imageStorage = Substitute.For<IImageStorage>();
        var writtenData = ReadOnlyMemory<byte>.Empty;

        await imageStorage.WriteDataAsync(Arg.Any<string>(), Arg.Do<ReadOnlyMemory<byte>>(x => writtenData = x));

        var imageWriter = new PpmImageWriter(imageStorage);

        var imageKey = "TestImage.ppn";
        const int imageWidth = 120;
        const int imageHeight = 62;
        var imageData = new Vector3[imageWidth * imageHeight];

        // Act
        await imageWriter.WriteImageAsync(imageKey, imageWidth, imageHeight, imageData);

        // Assert
        Assert.True(writtenData.Length > 0, "No data was written.");
        var output = ExtractTestOutputData(writtenData.Span);

        Assert.Equal("P3", output.Header);
        Assert.Equal(imageWidth, output.ImageWidth);
        Assert.Equal(imageHeight, output.ImageHeight);
        Assert.Equal(255, output.ImageMode);
    }

    [Fact]
    public async Task WriteImage_WithData_HasCorrectData()
    {
        // Arrange
        var imageStorage = Substitute.For<IImageStorage>();
        var writtenData = ReadOnlyMemory<byte>.Empty;

        await imageStorage.WriteDataAsync(Arg.Any<string>(), Arg.Do<ReadOnlyMemory<byte>>(x => writtenData = x));

        var imageWriter = new PpmImageWriter(imageStorage);

        var imageKey = "TestImage.ppn";
        const int imageWidth = 3;
        const int imageHeight = 2;

        var imageData = new Vector3[imageWidth * imageHeight]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1)
        };

        // Act
        await imageWriter.WriteImageAsync(imageKey, imageWidth, imageHeight, imageData);

        // Assert
        Assert.True(writtenData.Length > 0, "No data was written.");
        var output = ExtractTestOutputData(writtenData.Span);

        for (var i = 0; i < imageWidth * imageHeight; i++)
        {
            Assert.Equal(imageData[i], output.Data.Span[i]);
        }
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    public async Task WriteImage_WithZeroSize_ThrowsOutOfRangeException(int imageWidth, int imageHeight)
    {
        // Arrange
        var imageStorage = Substitute.For<IImageStorage>();
        var imageWriter = new PpmImageWriter(imageStorage);
        
        var imageKey = "TestImage.ppn";
        var imageData = new Vector3[imageWidth * imageHeight];

        // Act
        var action = async () => await imageWriter.WriteImageAsync(imageKey, imageWidth, imageHeight, imageData);

        // Arrange
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public async Task WriteImage_WithNoData_ThrowsOutOfRangeException()
    {
        // Arrange
        var imageStorage = Substitute.For<IImageStorage>();
        var imageWriter = new PpmImageWriter(imageStorage);
        
        var imageKey = "TestImage.ppn";
        const int imageWidth = 3;
        const int imageHeight = 2;
        var imageData = Array.Empty<Vector3>();

        // Act
        var action = async () => await imageWriter.WriteImageAsync(imageKey, imageWidth, imageHeight, imageData);

        // Arrange
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }

    private static PpmTestContent ExtractTestOutputData(ReadOnlySpan<byte> data)
    {
        var reader = new SpanReader(data);

        var headerValue = reader.ReadString('\r');
        var imageWidth = reader.ReadInt(' ');
        var imageHeight = reader.ReadInt('\r');
        var imageMode = reader.ReadInt('\r');

        var finalPixelData = new Vector3[imageWidth * imageHeight];

        for (var i = 0; i < imageWidth * imageHeight; i++)
        {
            var pixelRed = reader.ReadFloat(' ');
            var pixelGreen = reader.ReadFloat(' ');
            var pixelBlue = reader.ReadFloat('\r');

            finalPixelData[i] = new Vector3(pixelRed / 255.0f, pixelGreen / 255.0f, pixelBlue / 255.0f);
        }

        return new PpmTestContent
        {
            Header = headerValue,
            ImageWidth = imageWidth,
            ImageHeight = imageHeight,
            ImageMode = imageMode,
            Data = finalPixelData
        };
    }
}