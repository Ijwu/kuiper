namespace kuiper.Core.Services.Abstract
{
    public interface IPrecollectedHintSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken);
    }
}
