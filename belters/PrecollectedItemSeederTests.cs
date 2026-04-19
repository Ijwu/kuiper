using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

using Moq;

namespace belters;

public class PrecollectedItemSeederTests
{
    [Test]
    public async Task SeedAsync_WithPrecollectedItems_AddsExpectedReceivedItems()
    {
        // Arrange
        var receivedItemServiceMock = new Mock<IReceivedItemService>();
        var loggerMock = new Mock<ILogger<PrecollectedItemSeeder>>();
        var multiData = new MultiData
        {
            PrecollectedItems = new Dictionary<long, long[]>
            {
                { 2, [101, 102] },
                { 3, [201] }
            }
        };

        var sut = new PrecollectedItemSeeder(loggerMock.Object, multiData, receivedItemServiceMock.Object);

        // Act
        await sut.SeedAsync();

        // Assert
        receivedItemServiceMock.Verify(
            x => x.AddReceivedItemAsync(2, 0, It.Is<NetworkItem>(i =>
                i.Item == 101 &&
                i.Location == -2 &&
                i.Player == 2 &&
                i.Flags == NetworkItemFlags.None)),
            Times.Once);

        receivedItemServiceMock.Verify(
            x => x.AddReceivedItemAsync(2, 0, It.Is<NetworkItem>(i =>
                i.Item == 102 &&
                i.Location == -2 &&
                i.Player == 2 &&
                i.Flags == NetworkItemFlags.None)),
            Times.Once);

        receivedItemServiceMock.Verify(
            x => x.AddReceivedItemAsync(3, 0, It.Is<NetworkItem>(i =>
                i.Item == 201 &&
                i.Location == -2 &&
                i.Player == 3 &&
                i.Flags == NetworkItemFlags.None)),
            Times.Once);
    }

    [Test]
    public async Task SeedAsync_WithNoPrecollectedItems_DoesNotAddReceivedItems()
    {
        // Arrange
        var receivedItemServiceMock = new Mock<IReceivedItemService>();
        var loggerMock = new Mock<ILogger<PrecollectedItemSeeder>>();
        var multiData = new MultiData
        {
            PrecollectedItems = []
        };

        var sut = new PrecollectedItemSeeder(loggerMock.Object, multiData, receivedItemServiceMock.Object);

        // Act
        await sut.SeedAsync();

        // Assert
        receivedItemServiceMock.Verify(
            x => x.AddReceivedItemAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<NetworkItem>()),
            Times.Never);
    }
}
