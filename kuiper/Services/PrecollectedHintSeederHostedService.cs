using kuiper.Core.Services.Abstract;

namespace kuiper.Services
{
    public class PrecollectedHintSeederHostedService : IHostedService
    {
        private readonly IPrecollectedHintSeeder _precollectedHintSeeder;

        public PrecollectedHintSeederHostedService(IPrecollectedHintSeeder precollectedHintSeeder)
        {
            _precollectedHintSeeder = precollectedHintSeeder;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _precollectedHintSeeder.SeedAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
