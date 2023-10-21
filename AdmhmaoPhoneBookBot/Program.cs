global using ArveCore.Botf;
global using ArveCore.Extensions;
using AdmhmaoPhoneBookBot.IOControl;
using AdmhmaoPhoneBookBot.Services;

namespace AdmhmaoPhoneBookBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!File.Exists("appsettings.json"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error ] File appsettings.json is not exists");
                Console.ResetColor();
                return;
            }

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<InMemoryStorage>();
            builder.Services.AddHostedService<AppRuntimeService>();
            builder.Services.AddHostedService<PhoneBookUpdateService>();

            builder.Services.TryAddBotf(builder.Configuration.GetConnectionString("botf")!, default); //botf

            builder.WebHost.UseUrls("http://localhost:5002"); // Any ports (In default uses to auto setting webhooks to botf endpoint)

            var app = builder.Build();

            app.TryUseBotf(dropPendingUpdates: true); //botf

            app.Run();
        }
    }
}