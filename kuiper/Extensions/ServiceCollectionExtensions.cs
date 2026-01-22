using kuiper.Pickle;
using kuiper.Plugins;
using kuiper.Services;
using kuiper.Services.Abstract;
using kuiper.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace kuiper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKuiperServices(this IServiceCollection services, MultiData multiData)
        {
            services.AddSingleton<WebSocketConnectionManager>();
            services.AddSingleton<MultiData>(multiData);
            services.AddSingleton<PluginManager>();
            services.AddSingleton<IWebSocketHandler, WebSocketHandler>();

            services.AddSingleton<IStorageService, InMemoryStorageService>();
            services.AddSingleton<ILocationCheckService, LocationCheckService>();
            services.AddSingleton<IReceivedItemService, ReceivedItemService>();
            services.AddSingleton<IHintPointsService, HintPointsService>();
            services.AddSingleton<IServerAnnouncementService, ServerAnnouncementService>();
            services.AddSingleton<IHintService, HintService>();
            services.AddSingleton<IKuiperConfig, KuiperConfigService>(); // Add this

            services.AddHostedService<CommandLoopService>();

            return services;
        }

        public static IServiceCollection AddKuiperCommands(this IServiceCollection services)
        {
            services.AddSingleton<CommandRegistry>();
            services.AddSingleton<IConsoleCommand, HelpCommand>();
            services.AddSingleton<IConsoleCommand, SayCommand>();
            services.AddSingleton<IConsoleCommand, QuitCommand>();
            services.AddSingleton<IConsoleCommand, DumpStorageCommand>();
            services.AddSingleton<IConsoleCommand, HintCommand>();
            services.AddSingleton<IConsoleCommand, AuthorizeCommandSlot>();
            services.AddSingleton<IConsoleCommand, BackupStorageCommand>();
            services.AddSingleton<IConsoleCommand, RestoreStorageCommand>();
            services.AddSingleton<IConsoleCommand, ListSlotsCommand>();

            return services;
        }
    }
}
