using kuiper.Commands;
using kuiper.Commands.Abstract;
using kuiper.Core.Services;
using kuiper.Core.Services.Abstract;
using kuiper.Internal;
using kuiper.Plugins;
using kuiper.Services;

namespace kuiper.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKuiperServices(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionManager, WebSocketConnectionManager>();
            services.AddSingleton<IWebSocketHandler, WebSocketHandler>();
            services.AddSingleton<PluginManager>();

            services.AddSingleton<ILocationCheckService, LocationCheckService>();
            services.AddSingleton<IReceivedItemService, ReceivedItemService>();
            services.AddSingleton<IKuiperConfigService, KuiperConfigService>();
            services.AddSingleton<INotifyingStorageService, InMemoryNotifyingStorageService>();
            services.AddSingleton<IServerAnnouncementService, ServerAnnouncementService>();
            services.AddSingleton<IHintPointsService, HintPointsService>();
            services.AddSingleton<IHintService, HintService>();
            services.AddSingleton<IReleaseService, ReleaseService>();
            services.AddSingleton<IPrecollectedItemSeeder, PrecollectedItemSeeder>();
            services.AddSingleton<IPrecollectedHintSeeder, PrecollectedHintSeeder>();

            return services;
        }

        public static IServiceCollection AddKuiperCommands(this IServiceCollection services)
        {
            services.AddSingleton<ICommandRegistry, CommandRegistry>();

            services.AddTransient<ICommand, HelpCommand>();
            services.AddTransient<ICommand, QuitCommand>();
            services.AddTransient<ICommand, SayCommand>();
            services.AddTransient<ICommand, AuthorizeSlotCommand>();
            services.AddTransient<ICommand, ListSlotsCommand>();
            services.AddTransient<ICommand, BackupStorageCommand>();
            services.AddTransient<ICommand, RestoreStorageCommand>();
            services.AddTransient<ICommand, DumpStorageCommand>();

            return services;
        }

        public static IServiceCollection AddKuiperHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<PrecollectedItemSeederHostedService>();
            services.AddHostedService<PrecollectedHintSeederHostedService>();
            services.AddHostedService<CommandLoopService>();

            return services;
        }
    }
}
