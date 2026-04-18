# Copilot Instructions for kuiper

## 1. Project Overview

`kuiper` is a .NET 10 implementation of an Archipelago multiworld randomizer server. It communicates over WebSockets using the Archipelago protocol.

The server uses a plugin-first architecture: packet handling logic belongs in plugins, not in the host bootstrap. The system is intentionally extensible: third-party plugin assemblies (`.dll`) can be dropped into `Plugins/` and are loaded at startup.

Key projects in this repository:
- **`kuiper`** — host app (`Program.cs`), DI wiring (`Extensions/ServiceCollectionExtensions.cs`), middleware, and console command infrastructure.
- **`kuiper.Core`** — shared services/interfaces/models/constants/extensions.
- **`Plugins/`** — plugin assemblies (for example `kuiper.Core.Checks`, `kuiper.Core.Hints`, `kuiper.Core.DataStorage`) loaded dynamically from runtime `Plugins/` output.
- **`kbo`** — packet definitions (`bigrocks`) and shared structures/base classes (`littlerocks`).
- **`belters`** — unit tests.
- **`spaceport`** — client-side WebSocket connection library.
- **`stm`** — terminal UI test client.
- **`telescope`** — minimal console test client.

## 2. Plugin Architecture

- Every packet handler is a plugin.
- Plugins extend `BasePlugin` (`kuiper/Plugins/BasePlugin.cs`) and implement `IKuiperPlugin`.
- Packet handlers are declared in `RegisterHandlers()` via `Handle<TPacket>(handler)`. A plugin can handle multiple packet types.
- Reuse `BasePlugin` helper members instead of duplicating logic:
  - `Logger`
  - `ConnectionManager`
  - `SendToConnectionAsync(...)`
  - `SendToSlotAsync(...)`
  - `TryGetSlotForConnectionAsync(...)`
  - `GetSlotForConnectionAsync(...)`
- Dependencies are provided by constructor injection.
  - Depend on interfaces, not concrete types.
  - `MultiData` is the exception (plain data object without interface).
- Register plugins as transient `IKuiperPlugin`:
  - Core plugin/service wiring in `AddKuiperServices()`.
  - Dynamic plugins discovered in `Program.cs` via assembly scanning.
- `PluginManager` dispatches plugin handlers concurrently, with a per-plugin timeout of 5 seconds.
- Plugin handlers must be async and non-blocking.
- Do not place business logic in `Program.cs`.

## 3. Service and Interface Patterns (SOLID / DI)

- Every service has an interface in `kuiper.Core/Services/Abstract/`.
  - Naming: `IFooService` -> `FooService`.
- Service registrations live in one authoritative place:
  - `kuiper/Extensions/ServiceCollectionExtensions.cs` (`AddKuiperServices()` and related registration methods).
  - Do not register services elsewhere.
- Depend on interfaces in constructors for plugins/services.
- New cross-cutting startup/background behavior should be implemented as `IHostedService`/`BackgroundService` (see `CommandLoopService` pattern).
- Do not do async work in constructors; use `StartAsync`/`ExecuteAsync` or deferred async initialization.

## 4. Storage Conventions

- All persistent server state must go through `INotifyingStorageService`.
- Never bypass storage abstractions with direct/in-memory access.
- All storage keys must be defined in `kuiper.Core/Constants/StorageKeys.cs`.
  - Never inline storage key literals.
- Key shape: `#<category>:<entity>:<id>`.
  - Examples: `#received:slot:3`, `#hints:slot:3`, `#checks:slot:3`.
- Reserved prefixes:
  - `#` -> internal server keys
  - `_read_` -> Archipelago data storage read operations
- Storage is intentionally unified with Archipelago data storage.
- `DataStorageSetPlugin` contains guards preventing client writes to `_` and `#` prefixed keys.

## 5. Command Pattern

- Commands implement `ICommand` (`kuiper/Commands/Abstract/ICommand.cs`):
  - `Name`
  - `Description`
  - `Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken)`
- `executingSlot == -1` means server console execution.
- Any other `executingSlot` is the in-game slot ID of the player issuing the command via chat.
- Commands must enforce privilege checks using `executingSlot`.
- Register commands as transient `ICommand` in `AddKuiperCommands()`.
- In-game command dispatch is handled by `SayCommandPlugin`.
- Prefix is configurable via `Server:IngameCommandPrefix` in `config.json` (default `!`).

## 6. Logging Conventions

- Use `ILogger<T>` injected by DI.
- Do not use `Console.Write*` or `Log.Logger` directly in plugins/services.
- Verbosity expectations:
  - `LogInformation` -> lifecycle milestones
  - `LogDebug` -> per-operation details
  - `LogWarning` -> recoverable unexpected states
  - `LogError` -> correctness-impacting failures/exceptions
- Use structured logging placeholders:
  - ✅ `_logger.LogDebug("Handled {PacketType} from {ConnectionId}", packetType, connectionId)`
  - ❌ string interpolation inside log calls

## 7. Naming Conventions

- Interfaces: `IFooService`, `IFooPlugin`, `IFooCommand`
- Implementations: `FooService`, `FooPlugin`, `FooCommand`
- Private fields: `_camelCase`
- Async methods: suffix `Async`
- Plugin handler methods: `Handle<PacketType>Async` (for example `HandleLocationChecksAsync`)
- Storage key constants/factories in `StorageKeys`: `PascalCase` names, `PascalCase(long id)` factory methods
- Packet registration override name: `RegisterHandlers()`

## 8. Async and Concurrency

- All I/O and service calls should be async/await.
- Do not use `.Result` or `.Wait()`.
- `ConfigureAwait(false)` is encouraged in library-level code (for example `PluginManager`); optional in app/plugin-level code when context makes sense.
- Plugin handlers run concurrently; do not assume ordering across plugins for the same packet.

## 9. What to Avoid

- Do not add business logic to `Program.cs` (keep it focused on app configuration and DI/bootstrap).
- Do not bypass `INotifyingStorageService`.
- Do not duplicate slot/send helper logic from `BasePlugin`.
- Do not register services outside `ServiceCollectionExtensions`.
- Do not use `Console.ReadLine`/`Console.Write*` outside `CommandLoopService`.
- Do not cast `MultiData` slot IDs to `int` when `long` is expected.
- Do not inline storage key strings; use `StorageKeys.*`.
