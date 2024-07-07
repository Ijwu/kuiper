using System.Text.Json;

using kbo.bigrocks;
using kbo.littlerocks;

using NUnit.Framework.Internal;

namespace belters;

public class JsonMessagePartSerializationTests
{
    [Test]
    public void Read_ShouldDeserialize_Basic()
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
        PrintJson deserializedPacket = JsonSerializer.Deserialize<PrintJson>(json)!;
        JsonMessagePart.Text messagePart = deserializedPacket!.Data.First();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deserializedPacket, Is.Not.Null);
            Assert.That(deserializedPacket, Is.AssignableTo<Packet>());
            Assert.That(deserializedPacket, Is.TypeOf<PrintJson>());
            Assert.That(messagePart.Value, Is.EqualTo(messageString));
        });
    }
}

