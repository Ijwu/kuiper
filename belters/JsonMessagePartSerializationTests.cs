using System.Text.Json;

using kbo.bigrocks;
using kbo.littlerocks;

using NUnit.Framework.Internal;

namespace belters;

public class JsonMessagePartSerializationTests
{
    [Test]
    public void DeserializePrintJson_BasicMessage_Success()
    {
        // Arrange
        string messageString = "Now that you are connected, you can use !help to list commands to run via the server. If your client supports it, you may have additional local commands you can list with /help.";
        string json = $$"""
        {
            "cmd":"PrintJSON",
            "data":[
                {
                    "text":"{{messageString}}"
                }
            ],
            "type":"Tutorial"
        }
        """;

        // Act
        JsonSerializerOptions options = new() { AllowOutOfOrderMetadataProperties = true };
        PrintJson deserializedPacket = JsonSerializer.Deserialize<PrintJson>(json, options)!;
        JsonMessagePart.Text messagePart = deserializedPacket!.Data.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deserializedPacket, Is.Not.Null);
            Assert.That(deserializedPacket, Is.AssignableTo<Packet>());
            Assert.That(deserializedPacket, Is.TypeOf<TutorialPrintJson>());
            Assert.That(messagePart.Value, Is.EqualTo(messageString));
        });
    }

    [Test]
    public void DeserializePrintJson_CompositeMessage_Success()
    {
        // Arrange
        string json = $$"""
        {
            "cmd":"PrintJSON",
            "data":[
                {"type":"player_id","text":"1"},
                {"text":" found their "},
                {"type":"item_id","text":"1909085","player":1,"flags":1},
                {"text":" ("},
                {"type":"location_id","text":"1909002","player":1},
                {"text":")"}
            ],
            "type":"ItemSend",
            "receiving":1,
            "item": {
                "item":1909085,
                "location":1909002,
                "player":1,
                "flags":1,
                "class":"NetworkItem"
            }
        }
        """;

        // Act
        JsonSerializerOptions options = new() { AllowOutOfOrderMetadataProperties = true };
        PrintJson deserializedPacket = JsonSerializer.Deserialize<PrintJson>(json, options)!;
        var data = deserializedPacket!.Data;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deserializedPacket, Is.Not.Null);
            Assert.That(deserializedPacket, Is.AssignableTo<Packet>());
            Assert.That(deserializedPacket, Is.TypeOf<ItemSendPrintJson>());
            
            Assert.That(data[0], Is.TypeOf<JsonMessagePart.PlayerId>());
            var first = (data[0] as JsonMessagePart.PlayerId)!;
            Assert.That(first.Value, Is.EqualTo("1"));
            
            Assert.That(data[1], Is.TypeOf<JsonMessagePart.Text>());
            Assert.That(data[1].Value, Is.EqualTo(" found their "));
            
            Assert.That(data[2], Is.TypeOf<JsonMessagePart.ItemId>());
            var third = (data[2] as JsonMessagePart.ItemId)!;
            Assert.That(third.Value, Is.EqualTo("1909085"));
            Assert.That(third.Player, Is.EqualTo(1));
            Assert.That(third.Flags, Is.EqualTo(NetworkItemFlags.Advancement));

            Assert.That(data[3], Is.TypeOf<JsonMessagePart.Text>());
            Assert.That(data[3].Value, Is.EqualTo(" ("));

            Assert.That(data[4], Is.TypeOf<JsonMessagePart.LocationId>());
            var fifth = (data[4] as JsonMessagePart.LocationId)!;
            Assert.That(fifth.Value, Is.EqualTo("1909002"));
            Assert.That(fifth.Player, Is.EqualTo(1));

            Assert.That(data[5], Is.TypeOf<JsonMessagePart.Text>());
            Assert.That(data[5].Value, Is.EqualTo(")"));
        });   
    }
}

