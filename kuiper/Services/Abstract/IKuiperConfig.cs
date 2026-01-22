namespace kuiper.Services.Abstract
{
    public interface IKuiperConfig
    {
        T? GetPluginConfig<T>(string sectionName);
        T GetServerConfig<T>(string sectionName);
    }
}
