using AdmhmaoPhoneBookBot.IOControl;

namespace AdmhmaoPhoneBookBot.Services
{
    public class PhoneBookUpdateService : BackgroundService
    {
        private readonly InMemoryStorage _storage;
        public PhoneBookUpdateService(InMemoryStorage storage)
        {
            _storage = storage;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
