using System.Text.Json;

namespace belters;

public class PacketConverterTests
{
    [Test]
    public void TestRoomInfoWrite()
    {
        var roomInfo = new RoomInfo(
            new Version(1, 2, 3),
            new Version(4, 5, 6),
            new[] { "tag1", "tag2" },
            true,
            new Dictionary<string, int> { { "perm1", 1 }, { "perm2", 2 } },
            3,
            4,
            new[] { "game1", "game2" },
            new Dictionary<string, int> { { "dpv1", 1 }, { "dpv2", 2 } },
            new Dictionary<string, string> { { "dpc1", "c1" }, { "dpc2", "c2" } },
            "seed",
            1234567890
        );

        var json = JsonSerializer.Serialize(roomInfo, new JsonSerializerOptions());
        Assert.That(json, Is.EqualTo(@"{""version"":""1.2.3"",""generator_version"":""4.5.6"",""tags"":[""tag1"",""tag2""],""password"":true,""permissions"":{""perm1"":1,""perm2"":2},""hint_cost"":3,""location_check_points"":4,""games"":[""game1"",""game2""],""datapackage_versions"":{""dpv1"":1,""dpv2"":2},""datapackage_checksums"":{""dpc1"":""c1"",""dpc2"":""c2""},""seed_name"":""seed"",""time"":1234567890,""cmd"":""RoomInfo""}"));
    }

    [Test]
    public void TestRoomInfoRead()
    {
        var json = @"{""version"":""1.2.3"",""generator_version"":""4.5.6"",""tags"":[""tag1"",""tag2""],""password"":true,""permissions"":{""perm1"":1,""perm2"":2},""hint_cost"":3,""location_check_points"":4,""games"":[""game1"",""game2""],""datapackage_versions"":{""dpv1"":1,""dpv2"":2},""datapackage_checksums"":{""dpc1"":""c1"",""dpc2"":""c2""},""seed_name"":""seed"",""time"":1234567890,""cmd"":""RoomInfo""}";
        var roomInfo = JsonSerializer.Deserialize<RoomInfo>(json, new JsonSerializerOptions())!;

        Assert.That(roomInfo.Version, Is.EqualTo(new Version(1, 2, 3)));
        Assert.That(roomInfo.GeneratorVersion, Is.EqualTo(new Version(4, 5, 6)));
        Assert.That(roomInfo.Tags, Is.EqualTo(new[] { "tag1", "tag2" }));
        Assert.That(roomInfo.HasPassword, Is.True);
        Assert.That(roomInfo.Permissions, Is.EqualTo(new Dictionary<string, int> { { "perm1", 1 }, { "perm2", 2 } }));
        Assert.That(roomInfo.HintCost, Is.EqualTo(3));
        Assert.That(roomInfo.LocationCheckPoints, Is.EqualTo(4));
        Assert.That(roomInfo.Games, Is.EqualTo(new[] { "game1", "game2" }));
        Assert.That(roomInfo.DataPackageVersions, Is.EqualTo(new Dictionary<string, int> { { "dpv1", 1 }, { "dpv2", 2 } }));
        Assert.That(roomInfo.DataPackageChecksums, Is.EqualTo(new Dictionary<string, string> { { "dpc1", "c1" }, { "dpc2", "c2" } }));
        Assert.That(roomInfo.SeedName, Is.EqualTo("seed"));
        Assert.That(roomInfo.Time, Is.EqualTo(1234567890));
    }
}