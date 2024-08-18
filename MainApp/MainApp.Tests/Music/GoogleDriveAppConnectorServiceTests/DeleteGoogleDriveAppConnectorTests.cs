using Grpc.Core;
using MainApp.Protos;
using Moq;

namespace MainApp.Tests.Music.GoogleDriveAppConnectorServiceTests
{
    public class DeleteGoogleDriveAppConnectorTests : BaseGoogleDriveAppConnectorTests
    {
        [Fact]
        public async Task DeleteFile_ReturnsTrue()
        {
            // Arrange
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall(new DeleteResponse() { State = true });

            _mockClient
                .Setup(m => m.DeleteFileAsync(
                    It.IsAny<DeleteRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            var result = await _googleConnectorService.DeleteFile(trackId);

            // Assert
            Assert.True(result);

            _mockClient.Verify(c => c.DeleteFileAsync(
                It.Is<DeleteRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DeleteFile_ReturnsFalse()
        {
            // Arrange
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall(new DeleteResponse() { State = false });

            _mockClient
                .Setup(m => m.DeleteFileAsync(
                    It.IsAny<DeleteRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            var result = await _googleConnectorService.DeleteFile(trackId);

            // Assert
            Assert.False(result);

            _mockClient.Verify(c => c.DeleteFileAsync(
                It.Is<DeleteRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DeleteFile_NotDeleteFile_ThrowException()
        {
            // Arrange
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall<DeleteResponse>(StatusCode.NotFound);

            _mockClient
                .Setup(m => m.DeleteFileAsync(
                    It.IsAny<DeleteRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            await Assert.ThrowsAsync<RpcException>(() => _googleConnectorService.DeleteFile(trackId));

            // Assert
            _mockClient.Verify(c => c.DeleteFileAsync(
                It.Is<DeleteRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }
    }
}
