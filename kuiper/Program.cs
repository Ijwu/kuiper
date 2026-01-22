using Microsoft.Extensions.Hosting;
using kuiper;
using kuiper.Pickle;
using kuiper.Plugins;
using kuiper.Services;
using kuiper.Services.Abstract;
using kuiper.Commands;
using kuiper.Extensions;
using kuiper.Middleware; // Add this

using Razorvine.Pickle;

using Serilog;
using Serilog.Events;
using kbo.littlerocks;
using NativeFileDialogSharp;

Unpickler.registerConstructor("NetUtils", "SlotType", new SlotTypeObjectConstructor());
Unpickler.registerConstructor("NetUtils", "NetworkSlot", new MultiDataNetworkSlotObjectConstructor());

string? multidataFile = args.Length > 0 ? args[0] : null;

if (string.IsNullOrEmpty(multidataFile) || !File.Exists(multidataFile))
{
    var result = Dialog.FileOpen("archipelago");
    if (result.IsOk)
    {
        multidataFile = result.Path;
    }
    else
    {
        Console.WriteLine("No file selected. Exiting.");
        return;
    }
}

var fs = new FileStream(multidataFile, FileMode.Open);
var multiData = MultidataUnpickler.Unpickle(fs);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);

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

builder.Services.AddKuiperServices(multiData)
                .AddKuiperCommands();

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
pluginManager.RegisterBuiltInPlugins();

pluginManager.Initialize(app.Services);

// Preload precollected items as checks
await PreloadPrecollectedItemsAsync(app.Services, logger);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseWebSockets();

app.UseMiddleware<KuiperWebSocketMiddleware>();

app.Run();

static async Task PreloadPrecollectedItemsAsync(IServiceProvider services, Microsoft.Extensions.Logging.ILogger logger)
{
    try
    {
        using var scope = services.CreateScope();
        var multiData = scope.ServiceProvider.GetRequiredService<MultiData>();
        var locationCheckService = scope.ServiceProvider.GetRequiredService<ILocationCheckService>();
        var receivedItemService = scope.ServiceProvider.GetRequiredService<IReceivedItemService>();

        if (multiData.PrecollectedItems is null || multiData.PrecollectedItems.Count == 0)
            return;

        int totalChecks = 0;
        foreach (var kvp in multiData.PrecollectedItems)
        {
            var slot = kvp.Key;
            var itemIds = kvp.Value;

            if (!multiData.Locations.TryGetValue(slot, out var slotLocations))
                continue;

            foreach (var itemId in itemIds)
            {
                var item = new NetworkItem(itemId, 0, slot, NetworkItemFlags.None);
                await receivedItemService.AddReceivedItemAsync(item.Player, 0, item);
                totalChecks++;   
            }
        }

        logger.LogInformation("Preloaded {TotalChecks} precollected items for {SlotCount} slots", totalChecks, multiData.PrecollectedItems.Count);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to preload precollected items");
    }
}
