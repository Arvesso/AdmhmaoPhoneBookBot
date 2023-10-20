global using ArveCore.Botf;
global using ArveCore.Extensions;

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

            builder.Services.TryAddBotf(builder.Configuration.GetConnectionString("botf")!, default); //botf

            builder.WebHost.UseUrls("http://localhost:5002"); // Any ports (In default uses to auto setting webhooks to botf endpoint)

            var app = builder.Build();

            app.TryUseBotf(dropPendingUpdates: true); //botf

            app.Run();
        }
    }
}