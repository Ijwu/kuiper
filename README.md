# Kuiper

A .NET implementation of an [Archipelago](https://archipelago.gg/) multiworld randomizer server and client library.

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

The server takes a single argument: the path to a `.archipelago` file to host. There are no other accepted parameters at this time. A `.archipelago` file can be found inside the zipfile that the Archipelago Generator outputs upon generation.

Open the `config.json` file and explore the config. There isn't much there yet so it should be self-explanatory, and I will eventually document the options in the README. Also info on config options is in the changelog on the releases page.

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
The server uses a plugin architecture to handle incoming packets. Each plugin is responsible for handling one or more packet types. Plugins implement the `IKuiperPlugin` interface. When a packet is received, the server delegates the handling of the packet to each plugin. Each plugin decides if it is interested in the packet and handles it accordingly.

All files with the extension `.dll` in the `Plugins` directory will be loaded as a .NET assembly and dynamically searched for new plugins or commands. 

Commands implement the `ICommand` type and are used to provide additional console commands for the server.

Default Plugins:
- **kuiper.Core.Bounces** - Handles Bounce packets.
- **kuiper.Core.Chats** - Enables players to chat and run commands via the chat system.
- **kuiper.Core.Checks** - Enables location check tracking and Sync packet handlers. Enables the release command. This is the functionality you play AP for.
- **kuiper.Core.Connections** - Responds to Connect packets and allows players to set their slot password.
- **kuiper.Core.DataPackages** - Responds to requests for the data package.
- **kuiper.Core.DataStorage** - Responds to all requests having to do with the data storage system.
- **kuiper.Core.Hints** - Handles hint tracking, updating, and enables the hint command.


None of the default plugins are technically required for the server to function, however you will miss out on the functionality covered by the plugin. The default plugins are meant to cover the base functionality expected from an Archipelago server.

## Background / Motivations

I wanted to use this project as a way to spend free time messing about with AI. Almost all of the code in the `kuiper` project is touched in some way or another by AI. 

V2 was me going through the code and refactoring into plugins and hand-fixing most of the AI slop.