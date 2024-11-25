using kbo.bigrocks;
using kbo.littlerocks;

using Moq;

using spaceport;
using spaceport.schematics;

namespace belters;

public class ReceivingBayTests
{
    [Test]
    public void SyncHandler_RegisterAndCallSuccessfully()
    {
        // Arrange
        ITalkToTheServer mockServerTalker = Mock.Of<ITalkToTheServer>( mst =>
            mst.IsConnected == true &&
            mst.ReceivePacketsAsync(It.IsAny<CancellationToken>()) == Task.FromResult<Packet[]>(new Packet[]{new Say("Test")})
        );
        ReceivingBay bay = new ReceivingBay(mockServerTalker);

        // Act
        var packetHandlerMock = new Mock<PacketsReceivedHandler>();
        var handlerHandle = bay.OnPacketsReceived(packetHandlerMock.Object);
        bay.StartReceiving();

        // Assert
        packetHandlerMock.Verify(phm => phm(new Packet[]{new Say("Test")}), Times.AtLeastOnce);
    }
}
