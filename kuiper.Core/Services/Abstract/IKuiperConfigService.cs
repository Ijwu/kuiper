namespace kuiper.Core.Services.Abstract
{
    public interface IKuiperConfigService
    {
        T? GetPluginConfig<T>(string sectionName);
        T? GetServerConfig<T>(string sectionName);
    }
}
