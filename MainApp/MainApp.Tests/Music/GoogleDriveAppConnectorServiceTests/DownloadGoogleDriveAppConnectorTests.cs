using Google.Protobuf;
using Grpc.Core;
using MainApp.Protos;
using Moq;

namespace MainApp.Tests.Music.GoogleDriveAppConnectorServiceTests
{
    public class DownloadGoogleDriveAppConnectorTests : BaseGoogleDriveAppConnectorTests
    {
        [Fact]
        public async Task DownloadFile_DownloadFile_ReturnsByteArray()
        {
            // Arrange
            var trackId = "testTrack";
            var fileData = ByteString.CopyFrom(new byte[] { 1, 2, 3 });
            var mockCall = CallHelpers.CreateAsyncUnaryCall(new DownloadResponse() { FileData = fileData });

            _mockClient
                .Setup(m => m.DownloadFileStreamAsync(
                    It.IsAny<DownloadRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            var result = await _googleConnectorService.DownloadFile(trackId);

            // Assert
            Assert.NotNull(result);
            var resultBytes = new byte[3];
            await result!.ReadAsync(resultBytes, 0, resultBytes.Length);
            Assert.Equal(new byte[] { 1, 2, 3 }, resultBytes);

            _mockClient.Verify(c => c.DownloadFileStreamAsync(
                It.Is<DownloadRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task DownloadFile_NotDownloadFile_ThrowException()
        {
            // Arrange
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall<DownloadResponse>(StatusCode.NotFound);
            _mockClient
                .Setup(m => m.DownloadFileStreamAsync(
                    It.IsAny<DownloadRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            await Assert.ThrowsAsync<RpcException>(() => _googleConnectorService.DownloadFile(trackId));

            // Assert
            _mockClient.Verify(c => c.DownloadFileStreamAsync(
                It.Is<DownloadRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }
    }
}
