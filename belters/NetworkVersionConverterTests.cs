using kbo.plantesimals;
using System.Text.Json;

namespace belters
{
    public class NetworkVersionConverterTests
    {
        [Test]
        public void Read_ShouldDeserializeVersionCorrectly()
        {
            // Arrange
            var converter = new NetworkVersionConverter();
            var json = "{\"major\": 1, \"minor\": 2, \"build\": 3}";

            // Act
            var version = JsonSerializer.Deserialize<Version>(json, new JsonSerializerOptions { Converters = { converter } })!;
            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(version.Major, Is.EqualTo(1));
                Assert.That(version.Minor, Is.EqualTo(2));
                Assert.That(version.Build, Is.EqualTo(3));
            });
        }

        [Test]
        public void Read_ShouldDeserializeVersionCorrectly_WithClassProperty()
        {
            // Arrange
            var converter = new NetworkVersionConverter();
            var json = "{\"major\": 1, \"minor\": 2, \"build\": 3, \"class\": \"Version\"}";

            // Act
            var version = JsonSerializer.Deserialize<Version>(json, new JsonSerializerOptions { Converters = { converter } })!;
            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(version.Major, Is.EqualTo(1));
                Assert.That(version.Minor, Is.EqualTo(2));
                Assert.That(version.Build, Is.EqualTo(3));
            });
        }

        [Test]
        public void Write_ShouldSerializeVersionCorrectly()
        {
            // Arrange
            var converter = new NetworkVersionConverter();
            var version = new Version(1, 2, 3);
            var expectedJson = "{\"major\":1,\"minor\":2,\"build\":3,\"class\":\"Version\"}";

            // Act
            var jsonString = JsonSerializer.Serialize(version, new JsonSerializerOptions { Converters = { converter } });

            // Assert
            Assert.That(jsonString, Is.EqualTo(expectedJson));
        }
    }
}