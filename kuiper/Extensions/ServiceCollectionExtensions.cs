using kuiper.Commands;
using kuiper.Commands.Abstract;
using kuiper.Core.Services;
using kuiper.Core.Services.Abstract;
using kuiper.Internal;
using kuiper.Plugins;

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
            services.AddSingleton<IStorageService, InMemoryStorageService>();
            services.AddSingleton<IServerAnnouncementService, ServerAnnouncementService>();
            services.AddSingleton<IHintPointsService, HintPointsService>();

            return services;
        }

        public static IServiceCollection AddKuiperCommands(this IServiceCollection services)
        {
            services.AddSingleton<ICommandRegistry, CommandRegistry>();

            services.AddHostedService<CommandLoopService>();

            services.AddTransient<ICommand, HelpCommand>();

            return services;
        }
    }
}
