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

            return services;
        }
    }
}
