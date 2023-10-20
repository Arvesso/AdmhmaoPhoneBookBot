using AdmhmaoPhoneBookBot.IOControl;
using ArveCore;
using ArveCore.Culture;
using Telegram.Bot.Types.Enums;

namespace AdmhmaoPhoneBookBot.BotControllers
{
    public class MainController : BotController
    {
        private readonly InMemoryStorage _storage;
        private readonly ILogger _logger;
        public MainController(InMemoryStorage memoryStorage, ILogger<MainController> logger)
        {
            _logger = logger;
            _storage = memoryStorage;
        }

        #region States

        [State]
        private bool IsInitialized = false;

        #endregion

        #region SystemHandle

        [On(Handle.BeforeAll)]
        public async Task OnBeforeAll()
        {
            if (IsInitialized)
            {
                if (Context.Update.Type != UpdateType.CallbackQuery || LastSendedMessage is null)
                    return;

                if (Context.GetCallbackQuery().Message?.Date < LastSendedMessage.Date)
                {
                    await AnswerCallback("Устаревшая панель управления");
                    Context.StopHandling();
                }
            }
        }

        [On(Handle.Unknown)]
        public async Task OnUnknownAction()
        {
            if (Context.Update.Type == UpdateType.CallbackQuery)
            {
                await AnswerCallback("Неизвестный запрос");
            }

            await MainPanel();
            await Send();
        }

        [On(Handle.Exception)]
        public async Task OnException(Exception ex)
        {
            if (Context.Update.Type == UpdateType.CallbackQuery)
            {
                await AnswerCallback("Ошибка");
            }

            await MainPanel();
            await Send();

            _logger.LogWarning($"Bot task exception: {ex.Message}");
        }

        #endregion

        [Action("/start", "Start the bot")]
        public async Task Start()
        {
            Reply();
            PushL("Приветствую! Я помогу узнать расписание, воспользуйся панелью выбора");
            await Send();
            await MainPanel();
        }

        [Action("/panel", "Main panel")]
        public async Task MainPanel()
        {
            await ClearState();

            PushL($"<b>Расписание занятий</b>");
            PushL("");
            PushL($"Сегодня {CTimezone.Current.ToString("dddd, dd MMMM", CultureParameters.DefaultRu)}");
            PushL("");
            PushL(">");
            PushL();

            if (!IsInitialized)
                IsInitialized = true;
        }
    }
}
