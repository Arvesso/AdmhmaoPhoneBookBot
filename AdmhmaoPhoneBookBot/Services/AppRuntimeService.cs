using AdmhmaoPhoneBookBot.IOControl;
using ArveCore.Botf.Tracking;

namespace AdmhmaoPhoneBookBot.Services
{
    public class AppRuntimeService : IHostedService // Service for providing start and stop runtime methods 
    {
        private readonly InMemoryStorage _storage;
        public AppRuntimeService(InMemoryStorage storage)
        {
            _storage = storage;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TelegramResponseBlockBypass.InitHooks(); // Debug only!

            _storage.LoadPhoneBook();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            TelegramResponseBlockBypass.GracefulDisconnect();

            _storage.SerializePhoneBook();

            return Task.CompletedTask;
        }
    }
}
