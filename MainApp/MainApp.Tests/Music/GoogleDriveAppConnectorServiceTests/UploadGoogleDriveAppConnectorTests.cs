using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MainApp.Protos;
using Microsoft.AspNetCore.Http;
using Moq;

namespace MainApp.Tests.Music.GoogleDriveAppConnectorServiceTests
{
    public class UploadGoogleDriveAppConnectorTests : BaseGoogleDriveAppConnectorTests
    {
        [Fact]
        public async Task UploadFile_UploadSuccess()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall(new Empty());
            _mockClient
                .Setup(m => m.UploadFileAsync(
                    It.IsAny<UploadRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            await _googleConnectorService.UploadFile(mockFile.Object, trackId);

            // Assert
            _mockClient.Verify(c => c.UploadFileAsync(
                It.Is<UploadRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task UploadFile_UploadFailure_ThrowException()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall<Empty>(StatusCode.InvalidArgument);
            _mockClient
                .Setup(m => m.UploadFileAsync(
                    It.IsAny<UploadRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            await Assert.ThrowsAsync<RpcException>(() => _googleConnectorService.UploadFile(mockFile.Object, trackId));

            // Assert
            _mockClient.Verify(c => c.UploadFileAsync(
                It.Is<UploadRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }
    }
}
