using AdmhmaoPhoneBookBot.IOControl;

namespace AdmhmaoPhoneBookBot.Services
{
    public class AppRuntimeService : IHostedService // Service for loading and saving data
    {
        private readonly InMemoryStorage _storage;
        public AppRuntimeService(InMemoryStorage storage)
        {
            _storage = storage;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _storage.LoadPhoneBook();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _storage.SerializePhoneBook();

            return Task.CompletedTask;
        }
    }
}
