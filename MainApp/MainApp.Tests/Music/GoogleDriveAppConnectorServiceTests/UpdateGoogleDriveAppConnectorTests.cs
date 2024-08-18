using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MainApp.Protos;
using Microsoft.AspNetCore.Http;
using Moq;

namespace MainApp.Tests.Music.GoogleDriveAppConnectorServiceTests
{
    public class UpdateGoogleDriveAppConnectorTests : BaseGoogleDriveAppConnectorTests
    {
        [Fact]
        public async Task UpdateFile_UpdateSuccess()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall(new Empty());
            _mockClient.Setup(m => m.UpdateFileAsync(
                    It.IsAny<UpdateRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            await _googleConnectorService.UpdateFile(mockFile.Object, trackId);

            // Assert
            _mockClient.Verify(c => c.UpdateFileAsync(
                It.Is<UpdateRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task UpdateFile_UpdateFailure_ThrowException()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var trackId = "testTrack";
            var mockCall = CallHelpers.CreateAsyncUnaryCall<Empty>(StatusCode.InvalidArgument);
            _mockClient
                .Setup(m => m.UpdateFileAsync(
                    It.IsAny<UpdateRequest>(), null, null, CancellationToken.None))
                .Returns(mockCall);

            // Act
            await Assert.ThrowsAsync<RpcException>(() => _googleConnectorService.UpdateFile(mockFile.Object, trackId));

            // Assert
            _mockClient.Verify(c => c.UpdateFileAsync(
                It.Is<UpdateRequest>(r => r.FileId == trackId), null, null, CancellationToken.None),
                Times.Once);
        }
    }
}
