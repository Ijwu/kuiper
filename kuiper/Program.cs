using kuiper.Pickle;

using Razorvine.Pickle;

using Serilog;
using Serilog.Events;

Unpickler.registerConstructor("NetUtils", "SlotType", new SlotTypeObjectConstructor());
Unpickler.registerConstructor("NetUtils", "NetworkSlot", new MultiDataNetworkSlotObjectConstructor());

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);

var port = builder.Configuration.GetValue<int?>("Server:Port");
if (port.HasValue)
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(port.Value);
    });
}

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

string? multidataFile = args.Length > 0 ? args[0] : null;
if (multidataFile == null)
{
    var filePath = builder.Configuration.GetValue<string?>("Server:File");
    if (!string.IsNullOrEmpty(filePath))
    {
        multidataFile = filePath;
    }
    else
    {
        Log.Logger.Fatal("No multidata file path provided and none was set in the configuration file.");
        return;
    }
}
var fs = new FileStream(multidataFile, FileMode.Open);
var multiData = MultidataUnpickler.Unpickle(fs);



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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseWebSockets();

app.UseMiddleware<KuiperWebSocketMiddleware>();

app.Run();
