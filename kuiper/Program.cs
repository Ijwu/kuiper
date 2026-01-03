using Microsoft.Extensions.Hosting;
using kuiper;
using kuiper.Pickle;
using kuiper.Plugins;
using kuiper.Services;
using kuiper.Services.Abstract;
using kuiper.Commands;

using Razorvine.Pickle;

using Serilog;
using Serilog.Events;

Unpickler.registerConstructor("NetUtils", "SlotType", new SlotTypeObjectConstructor());
Unpickler.registerConstructor("NetUtils", "NetworkSlot", new MultiDataNetworkSlotObjectConstructor());

string multidataFile = @"C:\ProgramData\Archipelago\output\hk2players.archipelago";
//string multidataFile = @"C:\ProgramData\Archipelago\output\clique.archipelago";

var fs = new FileStream(multidataFile, FileMode.Open);
var multiData = MultidataUnpickler.Unpickle(fs);

var builder = WebApplication.CreateBuilder(args);

var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logsDir);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // keep framework noise lower
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logsDir, "app.log"),
                  retainedFileCountLimit: 14,
                  restrictedToMinimumLevel: LogEventLevel.Debug,
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<MultiData>(multiData);
builder.Services.AddSingleton<PluginManager>();
builder.Services.AddSingleton<IWebSocketHandler, WebSocketHandler>();

builder.Services.AddSingleton<IStorageService, InMemoryStorageService>();
builder.Services.AddSingleton<ILocationCheckService, LocationCheckService>();
builder.Services.AddSingleton<IHintPointsService, HintPointsService>();
builder.Services.AddSingleton<IServerAnnouncementService, ServerAnnouncementService>();
builder.Services.AddSingleton<IHintService, HintService>();

builder.Services.AddSingleton<CommandRegistry>();
builder.Services.AddSingleton<IConsoleCommand, HelpCommand>();
builder.Services.AddSingleton<IConsoleCommand, SayCommand>();
builder.Services.AddSingleton<IConsoleCommand, QuitCommand>();
builder.Services.AddSingleton<IConsoleCommand, DumpStorageCommand>();
builder.Services.AddSingleton<IConsoleCommand, HintCommand>();
builder.Services.AddSingleton<IConsoleCommand, AuthorizeCommandSlot>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();
var logger = app.Logger;

// Resolve PluginManager and initialize plugin instances. Register plugin types before calling Initialize.
var pluginManager = app.Services.GetRequiredService<PluginManager>();
// Register built-in plugins
pluginManager.RegisterPlugin<ConnectHandlerPlugin>();
pluginManager.RegisterPlugin<LocationChecksPlugin>();
pluginManager.RegisterPlugin<DataPackagePlugin>();
pluginManager.RegisterPlugin<DataStorageGetPlugin>();
pluginManager.RegisterPlugin<DataStorageSlotDataPlugin>();
pluginManager.RegisterPlugin<DataStorageSetPlugin>();
pluginManager.RegisterPlugin<DataStorageRaceModePlugin>();
pluginManager.RegisterPlugin<DataStorageNameGroupsPlugin>();
pluginManager.RegisterPlugin<DataStorageHintsPlugin>();
pluginManager.RegisterPlugin<LocationScoutsPlugin>();
pluginManager.RegisterPlugin<SyncPlugin>();
pluginManager.RegisterPlugin<ReleasePlugin>();
pluginManager.RegisterPlugin<ChatPlugin>();
pluginManager.RegisterPlugin<BouncePlugin>();
pluginManager.RegisterPlugin<CreateHintsPlugin>();
pluginManager.RegisterPlugin<SayCommandPlugin>();
pluginManager.RegisterPlugin<UpdateHintPlugin>();

pluginManager.Initialize(app.Services);

StartCommandLoop(app.Services, logger, app.Lifetime);
// Preload precollected items as checks
//await PreloadPrecollectedItemsAsync(app.Services, logger);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseWebSockets();

app.Map("/", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var connectionManager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
        var webSocketHandler = context.RequestServices.GetRequiredService<IWebSocketHandler>();

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        logger.LogInformation("Incoming Connection. Connection ID: {ConnectionId}", connectionId);

        var newPlayer = new PlayerData
        {
            Socket = webSocket
        };

        await connectionManager.AddConnectionAsync(connectionId, newPlayer);
        await webSocketHandler.HandleConnectionAsync(connectionId, newPlayer);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();

static void StartCommandLoop(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger, IHostApplicationLifetime lifetime)
{
    Task.Run(async () =>
    {
        try
        {
            using var scope = services.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<CommandRegistry>();
            var commands = scope.ServiceProvider.GetServices<IConsoleCommand>();
            foreach (var cmd in commands)
            {
                registry.Register(cmd);
            }

            logger.LogInformation("Command loop started. Type 'help' for commands.");

            while (!lifetime.ApplicationStopping.IsCancellationRequested)
            {
                var line = await Task.Run(() => Console.ReadLine(), lifetime.ApplicationStopping);
                if (line is null)
                {
                    await Task.Delay(100, lifetime.ApplicationStopping);
                    continue;
                }

                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var commandName = parts[0];
                var args = parts.Skip(1).ToArray();

                if (registry.TryGet(commandName, out var command))
                {
                    var result = await command.ExecuteAsync(args, scope.ServiceProvider, lifetime.ApplicationStopping);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        Console.WriteLine(result);
                    }
                }
                else
                {
                    Console.WriteLine("Unknown command. Type 'help' for list.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Command loop failed");
        }
    });
}

static async Task PreloadPrecollectedItemsAsync(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
{
    try
    {
        using var scope = services.CreateScope();
        var multiData = scope.ServiceProvider.GetRequiredService<MultiData>();
        var locationCheckService = scope.ServiceProvider.GetRequiredService<ILocationCheckService>();

        if (multiData.PrecollectedItems is null || multiData.PrecollectedItems.Count == 0)
            return;

        foreach (var kvp in multiData.PrecollectedItems)
        {
            var slot = kvp.Key;
            foreach (var locationId in kvp.Value)
            {
                await locationCheckService.AddCheckAsync(slot, locationId);
            }
        }

        logger.LogInformation("Preloaded precollected items for {SlotCount} slots", multiData.PrecollectedItems.Count);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to preload precollected items");
    }
}
