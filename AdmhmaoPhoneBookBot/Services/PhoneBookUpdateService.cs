using AdmhmaoPhoneBookBot.IOControl;
using AdmhmaoPhoneBookBot.PhoneBook;
using HtmlAgilityPack;
using System;

namespace AdmhmaoPhoneBookBot.Services
{
    public class PhoneBookUpdateService : BackgroundService
    {
        private const string UserAgentBypass = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        private const string PhoneBookUri = "https://admhmao.ru/organy-vlasti/telefonnyy-spravochnik-ogv-hmao/";

        private readonly InMemoryStorage _storage;
        private readonly TimeSpan _timeout;
        private readonly ILogger _logger;
        public PhoneBookUpdateService(InMemoryStorage storage, ILogger<PhoneBookUpdateService> logger)
        {
            _storage = storage;
            _timeout = TimeSpan.FromMinutes(5);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var entities = ParseEntities();

                    if (entities is not null)
                    {
                        _storage.ClearPhoneBook();
                        
                        foreach (var entity in entities)
                        {
                            _storage.AddEntityToPhoneBook(entity);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("An error has occured while updating PhoneBook: {0}", ex.Message);
                }

                await Task.Delay(_timeout, stoppingToken);
            }
        }
        private static List<Entity>? ParseEntities()
        {
            try
            {
                var web = new HtmlWeb() { UserAgent = UserAgentBypass };
                var doc = web.Load(PhoneBookUri);

                var sotrNodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'sotr')]");

                var result = new List<Entity>();

                if (sotrNodes is not null)
                {
                    var entityId = 0;

                    foreach (var sotrNode in sotrNodes)
                    {
                        var entity = new Entity();

                        var post = sotrNode.SelectSingleNode(".//div[@class='post']").InnerText.Trim();
                        var fio = sotrNode.SelectSingleNode(".//div[@class='fio']").InnerText.Trim();

                        entity.Id = entityId;
                        entity.Fio = fio;
                        entity.Post = post;

                        var fieldsNode = sotrNode.SelectSingleNode(".//div[@class='fields']");

                        var fields = fieldsNode.SelectNodes(".//div");

                        foreach (var field in fields)
                        {
                            var value = field.InnerText.Replace(Patterns.Nbsp, " ", StringComparison.OrdinalIgnoreCase).Trim();

                            if (Patterns.AnotherPhone.All(p => value.Contains(p, StringComparison.OrdinalIgnoreCase)))
                            {
                                entity.AnotherPhone = value;
                            }
                            else if (value.Contains(Patterns.Phone, StringComparison.OrdinalIgnoreCase))
                            {
                                entity.Phone = value;
                            }
                            else if (value.Contains(Patterns.Room, StringComparison.OrdinalIgnoreCase))
                            {
                                entity.Room = value;
                            }
                            else if (value.Contains(Patterns.Mail, StringComparison.OrdinalIgnoreCase))
                            {
                                entity.Mail = value;
                            }
                        }

                        result.Add(entity);
                        entityId++;
                    }
                }

                return result;
            }
            catch
            {
                return default;
            }          
        }
    }
}
