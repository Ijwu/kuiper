Disclaimer: This README is AI slop.
# Kuiper

A .NET implementation of an [Archipelago](https://archipelago.gg/) multiworld randomizer server and client library.

## Projects

| Project | Description | Target Framework |
|---------|-------------|------------------|
| **kuiper** | Archipelago server implementation with WebSocket support, plugin system, and console commands | .NET 10 |
| **kbo** | Core packet definitions and network protocol types (Kuiper Belt Objects) | .NET 9 |
| **spaceport** | Client library for connecting to Archipelago servers | .NET 9 |
| **stm** | Terminal UI client using Terminal.Gui (Space Terminal Module) | .NET 9 |
| **telescope** | Additional tooling | .NET 9 |
| **belters** | Unit tests | .NET 9 |

## Features

### Server (kuiper)

- **WebSocket Server**: Full Archipelago protocol support
- **Plugin Architecture**: Extensible packet handling system
- **Data Storage**: Key-value storage with Set/Get operations and subscriptions
- **Hint System**: Create, store, and broadcast hints to players
- **Console Commands**: Interactive server management
  - `help` - List available commands
  - `say <message>` - Broadcast server message
  - `hint <slotId> <item>` - Create hints by item name
  - `dumpkey [key]` - Inspect storage contents
  - `authslot <slotId>` - Authorize in-game commands
  - `quit` - Shutdown server
- **In-Game Commands**: Authorized players can run `!<command>` via chat

### Packets (kbo)

Complete implementation of Archipelago network protocol packets:
- Connection: `Connect`, `Connected`, `ConnectionRefused`, `RoomInfo`, `RoomUpdate`
- Gameplay: `LocationChecks`, `ReceivedItems`, `LocationScouts`, `LocationInfo`
- Data Storage: `Get`, `Set`, `SetNotify`, `Retrieved`, `SetReply`
- Communication: `Say`, `PrintJson`, `Bounce`, `Bounced`
- Hints: `CreateHints`, `UpdateHint`
- And more...

### Client Library (spaceport)

- `Freighter`: WebSocket client for server communication
- `ReceivingBay`: Packet receiving and handler registration

## Getting Started

### Prerequisites

- .NET 9 SDK (for client libraries)
- .NET 10 SDK (for server)

### Running the Server

```bash
cd kuiper
dotnet run
```

The server loads multidata from a `.archipelago` file (configure path in `Program.cs`).

### Using the Client Library

```csharp
using spaceport;
using kbo.bigrocks;

var client = new Freighter("ws://localhost:38281");
await client.ConnectAsync();

var connect = new Connect(
    password: null,
    game: "MyGame",
    name: "Player1",
    uuid: Guid.NewGuid().ToString(),
    version: new NetworkVersion(0, 5, 0),
    itemsHandling: 7,
    tags: Array.Empty<string>(),
    slotData: true
);

await client.SendPacketsAsync(new[] { connect });
```

## Architecture

```
kuiper/
├── Commands/          # Console command implementations
├── Pickle/            # Python pickle deserialization for multidata
├── Plugins/           # Packet handler plugins
└── Services/          # Core services (storage, hints, connections)

kbo/
├── bigrocks/          # Large packet types (Connect, etc.)
├── littlerocks/       # Small types (Hint, NetworkItem, etc.)
└── plantesimals/      # JSON converters

spaceport/
├── Freighter.cs       # WebSocket client
└── ReceivingBay.cs    # Packet handler registration
```

## License

See [LICENSE](LICENSE) for details.
