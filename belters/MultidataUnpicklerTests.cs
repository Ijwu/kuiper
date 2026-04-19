using System.Collections;
using System.Reflection;

using kuiper.Core.Pickle;

namespace belters;

public class MultidataUnpicklerTests
{
    [Test]
    public void GetPrecollectedHints_WithMultiDataHintValues_ReturnsOriginalHints()
    {
        // Arrange
        var hint = new MultiDataHint
        {
            ReceivingPlayer = 7,
            FindingPlayer = 1,
            Location = 123,
            Item = 456,
            Found = false,
            Entrance = string.Empty,
            ItemFlags = 0,
            Status = MultiDataHintStatus.NoPriority
        };

        Hashtable unpickledPrecollectedHints = new()
        {
            [7L] = new HashSet<object> { hint }
        };

        // Act
        var result = InvokeGetPrecollectedHints(unpickledPrecollectedHints);

        // Assert
        Assert.That(result[7L], Has.Length.EqualTo(1));
        Assert.That(result[7L][0], Is.EqualTo(hint));
    }

    private static Dictionary<long, MultiDataHint[]> InvokeGetPrecollectedHints(Hashtable unpickledPrecollectedHints)
    {
        var method = typeof(MultidataUnpickler).GetMethod("GetPrecollectedHints", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        return (Dictionary<long, MultiDataHint[]>)method!.Invoke(null, [unpickledPrecollectedHints])!;
    }
}
