using kuiper.Core.Services.Abstract;

namespace kuiper.Services
{
    public class PrecollectedItemSeederHostedService : IHostedService
    {
        private readonly IPrecollectedItemSeeder _precollectedItemSeeder;

        public PrecollectedItemSeederHostedService(IPrecollectedItemSeeder precollectedItemSeeder)
        {
            _precollectedItemSeeder = precollectedItemSeeder;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _precollectedItemSeeder.SeedAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
