using Grpc.Net.Client;
using MainApp.Protos;

namespace MainApp.Tests.Music.GoogleDriveAppConnectorServiceTests
{
    public class BaseGoogleDriveAppConnectorTests : IClassFixture<GrpcWebAppFactory>
    {
        protected readonly GoogleDriveConnector.GoogleDriveConnectorClient _client;

        public BaseGoogleDriveAppConnectorTests(GrpcWebAppFactory factory)
        {
            var channel = GrpcChannel.ForAddress(factory.Server.BaseAddress, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 16 * 1024 * 1024,
                MaxSendMessageSize = 16 * 1024 * 1024,
                HttpHandler = factory.Server.CreateHandler()
            });

            _client = new GoogleDriveConnector.GoogleDriveConnectorClient(channel);
        }
    }
}
