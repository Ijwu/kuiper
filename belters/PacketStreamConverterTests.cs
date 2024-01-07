using System.Text;
using System.Text.Json;
using kbo.bigrocks;
using kbo.littlerocks;
using kbo.plantesimals;

namespace belters;

public class PacketStreamConverterTests
{
    [Test]
    public void Read_ShouldDeserializePacketsCorrectly()
    {
        // Arrange
        var converter = new PacketStreamConverter();
        var options = new JsonSerializerOptions();

        var json = @"[{""cmd"":""RoomInfo"",""version"":{""major"":1,""minor"":2,""build"":3},""generator_version"":{""major"":4,""minor"":5,""build"":6},""tags"":[""tag1"",""tag2""],""password"":true,""permissions"":{""perm1"":0,""perm2"":1},""hint_cost"":3,""location_check_points"":4,""games"":[""game1"",""game2""],""datapackage_versions"":{""dpv1"":1,""dpv2"":2},""datapackage_checksums"":{""dpc1"":""c1"",""dpc2"":""c2""},""seed_name"":""seed"",""time"":1234567890},{""cmd"":""Connect"",""password"":""poop"",""game"":""poop"",""name"":""poop"",""uuid"":""00000000-0000-0000-0000-000000000000"",""version"":{""major"":1,""minor"":2,""build"":3},""items_handling"":0,""tags"":[""tag1"",""tag2""],""slot_data"":false}]";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var packets = converter.Read(ref reader, typeof(Packet[]), options);

        // Assert
        Assert.That(packets.Length, Is.EqualTo(2));

        Assert.That(packets[0], Is.InstanceOf<RoomInfo>());
        RoomInfo roomInfo = (RoomInfo)packets[0];
        Assert.That(roomInfo.Version, Is.EqualTo(new Version(1,2,3)));
        Assert.That(roomInfo.GeneratorVersion, Is.EqualTo(new Version(4,5,6)));
        Assert.That(roomInfo.Tags, Is.EquivalentTo(new string[] { "tag1", "tag2" }));
        Assert.That(roomInfo.HasPassword, Is.True);
        Assert.That(roomInfo.Permissions, Is.EquivalentTo(new Dictionary<string, CommandPermission> { { "perm1", CommandPermission.Disabled }, { "perm2", CommandPermission.Enabled } }));
        Assert.That(roomInfo.HintCost, Is.EqualTo(3));
        Assert.That(roomInfo.LocationCheckPoints, Is.EqualTo(4));
        Assert.That(roomInfo.Games, Is.EquivalentTo(new string[] { "game1", "game2" }));
        Assert.That(roomInfo.DataPackageVersions, Is.EquivalentTo(new Dictionary<string, int> { { "dpv1", 1 }, { "dpv2", 2 } }));
        Assert.That(roomInfo.DataPackageChecksums, Is.EquivalentTo(new Dictionary<string, string> { { "dpc1", "c1" }, { "dpc2", "c2" } }));
        Assert.That(roomInfo.SeedName, Is.EqualTo("seed"));
        Assert.That(roomInfo.Time, Is.EqualTo(1234567890));


        Assert.That(packets[1], Is.InstanceOf<Connect>());
        Connect connect = (Connect)packets[1];
        Assert.That(connect.Password, Is.EqualTo("poop"));
        Assert.That(connect.Game, Is.EqualTo("poop"));
        Assert.That(connect.Name, Is.EqualTo("poop"));
        Assert.That(connect.Uuid, Is.EqualTo(Guid.Empty));
        Assert.That(connect.Version, Is.EqualTo(new Version(1, 2, 3)));
        Assert.That(connect.ItemsHandling, Is.EqualTo(ItemHandlingFlags.None));
        Assert.That(connect.Tags, Is.EquivalentTo(new string[] { "tag1", "tag2" }));
        Assert.That(connect.SlotData, Is.False);
    }

    [Test]
    public void Write_ShouldSerializePacketsCorrectly()
    {
        // Arrange
        var converter = new PacketStreamConverter();
        var options = new JsonSerializerOptions();

        var packets = new Packet[]
        {
            new RoomInfo(new Version(1, 2, 3), new Version(4, 5, 6), new string[] { "tag1", "tag2" }, true, new Dictionary<string, CommandPermission> { { "perm1", CommandPermission.Disabled }, { "perm2", CommandPermission.Enabled } }, 3, 4, new string[] { "game1", "game2" }, new Dictionary<string, long> { { "dpv1", 1 }, { "dpv2", 2 } }, new Dictionary<string, string> { { "dpc1", "c1" }, { "dpc2", "c2" } }, "seed", 1234567890),
            new Connect("poop", "poop", "poop", Guid.Empty, new Version(1, 2, 3), 0, new string[] { "tag1", "tag2" }, false)
        };
        
        var ms = new MemoryStream();
        var writer = new Utf8JsonWriter(ms);

        // Act
        converter.Write(writer, packets, options);
        writer.Flush();
        var jsonString = Encoding.UTF8.GetString(ms.ToArray());
        Console.WriteLine(jsonString);
        // Assert
        var expectedJson = @"[{""cmd"":""RoomInfo"",""version"":{""major"":1,""minor"":2,""build"":3,""class"":""Version""},""generator_version"":{""major"":4,""minor"":5,""build"":6,""class"":""Version""},""tags"":[""tag1"",""tag2""],""password"":true,""permissions"":{""perm1"":0,""perm2"":1},""hint_cost"":3,""location_check_points"":4,""games"":[""game1"",""game2""],""datapackage_versions"":{""dpv1"":1,""dpv2"":2},""datapackage_checksums"":{""dpc1"":""c1"",""dpc2"":""c2""},""seed_name"":""seed"",""time"":1234567890},{""cmd"":""Connect"",""password"":""poop"",""game"":""poop"",""name"":""poop"",""uuid"":""00000000-0000-0000-0000-000000000000"",""version"":{""major"":1,""minor"":2,""build"":3,""class"":""Version""},""items_handling"":0,""tags"":[""tag1"",""tag2""],""slot_data"":false}]";
        Assert.That(jsonString, Is.EqualTo(expectedJson));
    }
}
