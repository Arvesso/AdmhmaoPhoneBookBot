using AdmhmaoPhoneBookBot.IOControl;
using ArveCore;
using ArveCore.Culture;
using Telegram.Bot.Types.Enums;

namespace AdmhmaoPhoneBookBot.BotControllers
{
    #region Records

    public record WaitEnterFio();
    public record WaitEnterPhone();
    public record WaitEnterRoom();
    public record WaitEnterPost();

    #endregion

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
            PushL("Приветствую! Я помогу узнать контакты органов власти Ханты-Мансийского автономного округа – Югры, воспользуйся панелью выбора");
            await Send();
            await MainPanel();
        }

        [Action("/panel", "Main panel")]
        public async Task MainPanel()
        {
            await ClearState();

            PushL($"<b>Телефонная книга</b>");
            PushL("");
            PushL($"Общее количество контактов книги: {_storage.GetPhoneBook.Count}");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Поиск по фамилии", Q(SelectFio));
            Button("Поиск по телефону", Q(SelectPhone));
            RowButton("Поиск по кабинету", Q(SelectRoom));
            Button("Поиск по должности", Q(SelectPost));
            RowButton("Информация", Q(Information));

            if (!IsInitialized)
                IsInitialized = true;
        }

        [Action]
        public async Task SelectFio()
        {
            PushL($"<b>Введи фамилию для поиска</b>");
            PushL("");
            PushL("Примеры корректного ввода (регистр не учитывается)");
            PushL("");
            PushL("• <i>уткин</i>");
            PushL("• <i>анатолий</i>");
            PushL("• <i>валерьевич</i>");
            PushL("• <i>анатолий валерьевич</i>");
            PushL("• <i>уткин анатолий валерьевич</i>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Отмена", Q(MainPanel));

            await State(new WaitEnterFio());
        }

        [State]
        public async ValueTask EnterFio(WaitEnterFio _)
        {

        }

        [Action]
        public void SelectPhone()
        {

        }

        [State]
        public async ValueTask EnterPhone(WaitEnterPhone _)
        {

        }

        [Action]
        public void SelectRoom()
        {

        }

        [State]
        public async ValueTask EnterRoom(WaitEnterRoom _)
        {

        }

        [Action]
        public void SelectPost()
        {

        }

        [State]
        public async ValueTask EnterPost(WaitEnterPost _)
        {

        }

        [Action]
        public void Information()
        {
            PushL("<b>Информация</b>");
            PushL("");
            PushL("• Данные телефонной книги актуальны, пока <a href='https://admhmao.ru/organy-vlasti/telefonnyy-spravochnik-ogv-hmao/'>основной сервис</a> активен");
            PushL("");
            PushL("• Данный бот разработан <a href='https://t.me/Arvesso'>Arvesso</a>, специально для <b>Студенческого Диджитал Многоборья Югры 2023</b>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Вернуться", Q(MainPanel));
        }
    }
}
