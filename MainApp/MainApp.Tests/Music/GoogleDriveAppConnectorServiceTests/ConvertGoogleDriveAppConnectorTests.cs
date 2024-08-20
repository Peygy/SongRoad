using Google.Protobuf;
using MainApp.Services.Music;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;

namespace MainApp.Tests.Music.GoogleDriveAppConnectorServiceTests
{
    public class ConvertGoogleDriveAppConnectorTests : BaseGoogleDriveAppConnectorTests
    {
        [Fact]
        public void ConvertToByteString_ShouldConvertFileCorrectly()
        {
            // Arrange
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            var mockFile = new Mock<IFormFile>();

            var stream = new MemoryStream(fileContent);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                    .Returns((Stream target, CancellationToken _) => stream.CopyToAsync(target));

            var method = typeof(GoogleDriveAppConnectorService)
                .GetMethod("ConvertToByteString", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { mockFile.Object };

            // Act
            var result = (ByteString)method.Invoke(_googleConnectorService, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(fileContent, result.ToByteArray());
        }

        [Fact]
        public void ConvertToByteString_EmptyFile_ShouldReturnEmptyByteString()
        {
            // Arrange
            var fileContent = new byte[0];
            var mockFile = new Mock<IFormFile>();

            var stream = new MemoryStream(fileContent);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                    .Returns((Stream target, CancellationToken _) => stream.CopyToAsync(target));

            var method = typeof(GoogleDriveAppConnectorService)
                .GetMethod("ConvertToByteString", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { mockFile.Object };

            // Act
            var result = (ByteString)method.Invoke(_googleConnectorService, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ConvertToByteString_LargeFile_ShouldConvertCorrectly()
        {
            // Arrange
            var largeFileContent = new byte[10 * 1024 * 1024];
            new Random().NextBytes(largeFileContent);
            var mockFile = new Mock<IFormFile>();

            var stream = new MemoryStream(largeFileContent);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), CancellationToken.None))
                    .Returns((Stream target, CancellationToken _) => stream.CopyToAsync(target));

            var method = typeof(GoogleDriveAppConnectorService)
                .GetMethod("ConvertToByteString", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { mockFile.Object };

            // Act
            var result = (ByteString)method.Invoke(_googleConnectorService, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ConvertToByteString_ShouldThrowExceptionOnNullFile()
        {
            // Arrange
            var method = typeof(GoogleDriveAppConnectorService)
                .GetMethod("ConvertToByteString", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { null };

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => method.Invoke(_googleConnectorService, parameters));

            Assert.IsType<NullReferenceException>(exception.InnerException);
        }
    }
}
