using System.Collections;
using System.Reflection;

using kuiper.Core.Pickle;

namespace belters;

public class MultidataUnpicklerTests
{
    [Test]
    public void GetPrecollectedHints_WithTupleValues_MapsToMultiDataHints()
    {
        // Arrange
        Hashtable unpickledPrecollectedHints = new()
        {
            [5L] = new HashSet<object>
            {
                new object[] { 5L, 3L, 1001L, 2002L, true, "Entrance", 1L, 30 },
                new object[] { 5L, 4L, 1002L, 2003L, false, "", 2L }
            }
        };

        // Act
        var result = InvokeGetPrecollectedHints(unpickledPrecollectedHints);

        // Assert
        Assert.That(result.ContainsKey(5L), Is.True);
        Assert.That(result[5L], Has.Length.EqualTo(2));

        var firstHint = result[5L].Single(x => x.FindingPlayer == 3L);
        Assert.That(firstHint.ReceivingPlayer, Is.EqualTo(5L));
        Assert.That(firstHint.Location, Is.EqualTo(1001L));
        Assert.That(firstHint.Item, Is.EqualTo(2002L));
        Assert.That(firstHint.Found, Is.True);
        Assert.That(firstHint.Entrance, Is.EqualTo("Entrance"));
        Assert.That(firstHint.ItemFlags, Is.EqualTo(1L));
        Assert.That(firstHint.Status, Is.EqualTo(MultiDataHintStatus.Priority));

        var secondHint = result[5L].Single(x => x.FindingPlayer == 4L);
        Assert.That(secondHint.Status, Is.EqualTo(MultiDataHintStatus.Unspecified));
    }

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
