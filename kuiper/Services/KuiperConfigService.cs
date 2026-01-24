using kuiper.Services.Abstract;

namespace kuiper.Services
{
    public class KuiperConfigService : IKuiperConfig
    {
        private readonly IConfiguration _configuration;

        public KuiperConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T? GetPluginConfig<T>(string sectionName)
        {
            return _configuration.GetSection($"Plugins:{sectionName}").Get<T>();
        }

        public T? GetServerConfig<T>(string sectionName)
        {
            return _configuration.GetSection(sectionName).Get<T>();
        }
    }
}
