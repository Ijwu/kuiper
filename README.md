# Kuiper

A .NET implementation of an [Archipelago](https://archipelago.gg/) multiworld randomizer server and client library.

## Projects

| Project | Description |
|---------|-------------|
| **kuiper** | Archipelago server implementation with WebSocket support, plugin system, and console commands |
| **kbo** | Core packet definitions and network protocol types (Kuiper Belt Objects) |
| **spaceport** | Client library for connecting to Archipelago servers |
| **stm** | Terminal UI client using Terminal.Gui (Space Traffic Management) |
| **telescope** | Additional tooling |
| **belters** | Unit tests |

## kuiper Features

- **WebSocket Server**: Minimal Archipelago protocol support for now. 
- **Plugin Architecture**: Extensible packet handling system (To be implemented.)
- **Data Storage**: Key-value storage with Set/Get operations
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

Or run the EXE, I guess.

The server takes a single argument: the path to a `.archipelago` file to host. There are no other accepted parameters at this time. A `.archipelago` file can be found inside the zipfile that the Archipelago Generator outputs upon generation.

### Server Commands

- `authslot <slotId>` - Authorizes a player slot to execute server commands over chat by using the `!<command> <arguments>` format.
    - Authorizing a slot allows ALL connections for that slot to run commands. That includes their TextClient if they are using it.
- `backupstorage <path>` - Saves the server's datastore (all state information) to a json file.
- `dumpkey <key>` - Pretty print the current value of a data storage key. If no key is supplied then it will list all keys instead.
- `help` - List all commands.
- `hint <slotId> <item name>` - Creates a hint for a slot based on item name. 
- `listslots` - Lists currently connected players and their mapped slots.
    - Output format: `<connectionId> => slot <slotId> (<playerName>) - <game> tags: <connectionTags>`
- `quit` - Shut down the server. **DOES NOT** auto save.
- `restorestorage <path>` - Restore the server's datastore from a json file.
- `say <message>` - Broadcast a chat message from the server to all players.

All `<path>` arguments are relative to the server directory. Absolute paths may be supplied, if you want.

### Data Storage System

The server stores all of its state information in the data storage system. Yes, the same data storage that is available to clients to modify and read. Currently there is no guard preventing clients from reading or modifying internal state keys.

Using the `backupstorage` command you can back up all of the server state to a json file. Example: `backupstorage friendly_sync.json`

Similarly, you may restore the server state from json with the `restorestorage` command. These commands are how you save and load your game before turning off the server.

A key tenant behind this project is the idea that all of the server's state information can be managed in the same way that the data storage system is managed. I am interested by the idea of them co-mingling. On a small scale this works well. I don't know how well this scales. I figure I can change it if I ever needed to.

## kuiper Server Architecture

### Plugins
The server uses a plugin architecture to handle incoming packets. Each plugin is responsible for handling one or more packet types. Plugins implement the `IPlugin` interface and register themselves with the `PluginManager` during server startup.
When a packet is received, the server delegates the handling of the packet to each plugin. Each plugin decides if it is interested in the packet and handles it accordingly.

The idea is to load plugins dynamically at runtime from a `Plugins` directory. This is not yet implemented.

## Background / Motivations

I wanted to use this project as a way to spend free time messing about with AI. Almost all of the code in the `kuiper` project is touched in some way or another by AI. When you read the code and ask "Ijwu why?!?" in some way, it's likely the answer is AI. All in all, better than I thought and now I can likely extract a spec from this version and get an agent to rebuild it. At least that's the current plan.

All of the other projects (`kbo`, `spaceport`, `stm`, etc.) are hand-written and not touched by AI (except for a missing packet the AI added to `kbo`). I was writing an AP packet handling library at first and it got mature enough that I thought I could make a server out of it too. 

Well, that's not entirely true, I did use MultiClient.NET at first and try to use its packet types. It didn't work out for a couple of reasons, namely that MultiClient.NET is a client library so it was a doomed effort from the start. After that failed, I moved my project, which was just called "PrototypeArchipelagoServer" at the time into this solution, which was already named "kuiper". I created the eponymous `kuiper` project and bolted all the protoype code in and retrofitted it to work with `kbo`.