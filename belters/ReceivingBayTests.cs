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
        Thread.Sleep(150);
        packetHandlerMock.Verify(phm => phm(new Packet[]{new Say("Test")}), Times.AtLeastOnce);
    }

    [Test]
    public void AsyncHandler_RegisterAndCallSuccessfully()
    {
        // Arrange
        ITalkToTheServer mockServerTalker = Mock.Of<ITalkToTheServer>(mst =>
            mst.IsConnected == true &&
            mst.ReceivePacketsAsync(It.IsAny<CancellationToken>()) == Task.FromResult<Packet[]>(new Packet[] { new Say("Test") })
        );
        ReceivingBay bay = new ReceivingBay(mockServerTalker);

        // Act
        var packetHandlerMock = new Mock<PacketsReceivedHandlerAsync>();
        var handlerHandle = bay.OnPacketsReceived(packetHandlerMock.Object);
        bay.StartReceiving();

        // Assert
        Thread.Sleep(150);
        packetHandlerMock.Verify(phm => phm(new Packet[] { new Say("Test") }), Times.AtLeastOnce);
    }

    [Test]
    public void SyncHandler_DisposeHandler_NoLongerCallsHandler()
    {
        // Arrange
        ITalkToTheServer mockServerTalker = Mock.Of<ITalkToTheServer>(mst =>
            mst.IsConnected == true &&
            mst.ReceivePacketsAsync(It.IsAny<CancellationToken>()) == Task.FromResult<Packet[]>(new Packet[] { new Say("Test") })
        );
        ReceivingBay bay = new ReceivingBay(mockServerTalker);

        // Act
        var packetHandlerMock = new Mock<PacketsReceivedHandler>();
        var handlerHandle = bay.OnPacketsReceived(packetHandlerMock.Object);
        bay.StartReceiving();
        handlerHandle.Dispose();

        // Assert
        Thread.Sleep(150);
        packetHandlerMock.Verify(phm => phm(new Packet[] { new Say("Test") }), Times.Never);
    }

    [Test]
    public void AsyncHandler_DisposeHandler_NoLongerCallsHandler()
    {
        // Arrange
        ITalkToTheServer mockServerTalker = Mock.Of<ITalkToTheServer>(mst =>
            mst.IsConnected == true &&
            mst.ReceivePacketsAsync(It.IsAny<CancellationToken>()) == Task.FromResult<Packet[]>(new Packet[] { new Say("Test") })
        );
        ReceivingBay bay = new ReceivingBay(mockServerTalker);

        // Act
        var packetHandlerMock = new Mock<PacketsReceivedHandlerAsync>();
        var handlerHandle = bay.OnPacketsReceived(packetHandlerMock.Object);
        bay.StartReceiving();
        handlerHandle.Dispose();

        // Assert
        Thread.Sleep(150);
        packetHandlerMock.Verify(phm => phm(new Packet[] { new Say("Test") }), Times.Never);
    }

    [Test]
    public void SyncHandler_ConcurrentRegistrations_RegisterAndCallSuccessfully()
    {
        // Arrange
        ITalkToTheServer mockServerTalker = Mock.Of<ITalkToTheServer>(mst =>
            mst.IsConnected == true &&
            mst.ReceivePacketsAsync(It.IsAny<CancellationToken>()) == Task.FromResult<Packet[]>(new Packet[] { new Say("Test") })
        );
        ReceivingBay bay = new ReceivingBay(mockServerTalker);

        // Act
        List<Mock<PacketsReceivedHandler>> mocks = new();
        List<Task<IDisposable>> registerTasks = new();
        for (int i = 0; i < 50; i++)
        {
            registerTasks.Add(Task.Run(() =>
            {
                var packetHandlerMock = new Mock<PacketsReceivedHandler>();
                mocks.Add(packetHandlerMock);

                var handlerHandle = bay.OnPacketsReceived(packetHandlerMock.Object);
                return Task.FromResult(handlerHandle);
            }));
        }
        
        Task.WaitAll(registerTasks.ToArray());
        bay.StartReceiving();

        // Assert
        Thread.Sleep(150);
        foreach (var mock in mocks)
        {
            mock.Verify(phm => phm(new Packet[] { new Say("Test") }), Times.AtLeastOnce);
        }
    }
}
