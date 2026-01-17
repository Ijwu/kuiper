# Kuiper

A .NET implementation of an [Archipelago](https://archipelago.gg/) multiworld randomizer server and client library.

## Projects

| Project | Description |
|---------|-------------|
| **kuiper** | Archipelago server implementation with WebSocket support, plugin system, and console commands |
| **kbo** | Core packet definitions and network protocol types (Kuiper Belt Objects) |
| **spaceport** | Client library for connecting to Archipelago servers |
| **stm** | Terminal UI client using Terminal.Gui (Space Terminal Module) |
| **telescope** | Additional tooling |
| **belters** | Unit tests |

## kuiper Features

- **WebSocket Server**: Full Archipelago protocol support
- **Plugin Architecture**: Extensible packet handling system
- **Data Storage**: Key-value storage with Set/Get operations and subscriptions
- **Hint System**: Create, store, and broadcast hints to players
- **Console Commands**: Interactive server management
- **In-Game Commands**: Authorized players can run `!<command>` via chat

## Getting Started

### Prerequisites
- .NET 10 SDK

### Running the Server

```bash
cd kuiper
dotnet run
```

The server loads multidata from a `.archipelago` file (configure path in `Program.cs`).

## kuiper Server Architecture

### Directory Structure

```
kuiper/
├── Commands/          # Console command implementations
├── Pickle/            # Python pickle deserialization for multidata
├── Plugins/           # Packet handler plugins
└── Services/          # Core services (storage, hints, connections)
```

### Key Components

#### Data Storage Service
- Acts as central storage for all state information.

#### Plugin System
- Modular packet handling for easy extension.