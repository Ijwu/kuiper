using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

using Moq;

namespace belters;

public class PrecollectedHintSeederTests
{
    [Test]
    public async Task SeedAsync_WithPrecollectedHints_AddsExpectedHints()
    {
        // Arrange
        var hintServiceMock = new Mock<IHintService>();
        var loggerMock = new Mock<ILogger<PrecollectedHintSeeder>>();
        var multiData = new MultiData
        {
            PrecollectedHints = new Dictionary<long, MultiDataHint[]>
            {
                {
                    2,
                    [
                        new MultiDataHint
                        {
                            ReceivingPlayer = 2,
                            FindingPlayer = 1,
                            Location = 1001,
                            Item = 2001,
                            Found = true,
                            Entrance = "Cave",
                            ItemFlags = (long)NetworkItemFlags.Advancement,
                            Status = MultiDataHintStatus.Found
                        }
                    ]
                },
                {
                    3,
                    [
                        new MultiDataHint
                        {
                            ReceivingPlayer = 3,
                            FindingPlayer = 2,
                            Location = 1002,
                            Item = 2002,
                            Found = false,
                            Entrance = string.Empty,
                            ItemFlags = (long)NetworkItemFlags.Useful,
                            Status = MultiDataHintStatus.Priority
                        }
                    ]
                }
            }
        };

        var sut = new PrecollectedHintSeeder(loggerMock.Object, multiData, hintServiceMock.Object);

        // Act
        await sut.SeedAsync();

        // Assert
        hintServiceMock.Verify(
            x => x.AddOrUpdateHintAsync(2, It.Is<Hint>(h =>
                h.ReceivingPlayer == 2 &&
                h.FindingPlayer == 1 &&
                h.Location == 1001 &&
                h.Item == 2001 &&
                h.Found &&
                h.Entrance == "Cave" &&
                h.ItemFlags == NetworkItemFlags.Advancement &&
                h.Status == HintStatus.Found)),
            Times.Once);

        hintServiceMock.Verify(
            x => x.AddOrUpdateHintAsync(3, It.Is<Hint>(h =>
                h.ReceivingPlayer == 3 &&
                h.FindingPlayer == 2 &&
                h.Location == 1002 &&
                h.Item == 2002 &&
                h.Found == false &&
                h.Entrance == string.Empty &&
                h.ItemFlags == NetworkItemFlags.Useful &&
                h.Status == HintStatus.Priority)),
            Times.Once);
    }

    [Test]
    public async Task SeedAsync_WithNoPrecollectedHints_DoesNotAddHints()
    {
        // Arrange
        var hintServiceMock = new Mock<IHintService>();
        var loggerMock = new Mock<ILogger<PrecollectedHintSeeder>>();
        var multiData = new MultiData
        {
            PrecollectedHints = []
        };

        var sut = new PrecollectedHintSeeder(loggerMock.Object, multiData, hintServiceMock.Object);

        // Act
        await sut.SeedAsync();

        // Assert
        hintServiceMock.Verify(
            x => x.AddOrUpdateHintAsync(It.IsAny<long>(), It.IsAny<Hint>()),
            Times.Never);
    }
}
