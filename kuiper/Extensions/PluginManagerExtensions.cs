using kuiper.Plugins;

namespace kuiper.Extensions
{
    public static class PluginManagerExtensions
    {
        public static void RegisterBuiltInPlugins(this PluginManager pluginManager)
        {
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
            pluginManager.RegisterPlugin<ConnectionTagsPlugin>();
        }
    }
}
