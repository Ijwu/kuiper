using System.Reflection;

using kuiper.Commands.Abstract;
using kuiper.Core.Pickle;
using kuiper.Extensions;
using kuiper.Middleware;
using kuiper.Plugins.Abstract;

using Serilog;
using Serilog.Events;

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

var logsDir = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logsDir);

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // keep framework noise lower
#endif
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logsDir, "kuiper.log"),
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

builder.Services.AddSingleton(multiData);
builder.Services.AddKuiperServices();
builder.Services.AddKuiperCommands();

var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");
Directory.CreateDirectory(pluginDir);

foreach (var file in Directory.EnumerateFiles(pluginDir, "*.dll"))
{
    Assembly? loadedAsm = null;
    try
    {
        loadedAsm = Assembly.LoadFile(file);
    }
    catch 
    {
        Log.Logger.Warning("Failed to load file as plugin: {FilePath}", file);
        continue;
    }

    builder.Services.Scan(scrutor =>
        scrutor
            .FromAssemblies(loadedAsm)
                .AddClasses(classes => classes.AssignableTo<IKuiperPlugin>())
                .As<IKuiperPlugin>()
                .WithTransientLifetime()
                .AddClasses(classes => classes.AssignableTo<ICommand>())
                .As<ICommand>()
                .WithTransientLifetime()
    );
    Log.Logger.Information("Loaded file as plugin: {FilePath}", file);
}

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

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseWebSockets();

app.UseMiddleware<KuiperWebSocketMiddleware>();

app.Run();
