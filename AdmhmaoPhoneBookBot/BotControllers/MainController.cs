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
    public record WaitEnterDynamic();

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
            PushL($"Общее количество контактов книги:  {_storage.GetPhoneBook.Count}");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Поиск по фамилии", Q(SelectFio));
            Button("Поиск по телефону", Q(SelectPhone));
            RowButton("Поиск по кабинету", Q(SelectRoom));
            Button("Поиск по должности", Q(SelectPost));
            RowButton("Динамический поиск", Q(SelectDynamic));
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
            var entities = _storage.FindBestMatchesByFio(Context.GetSafeTextPayload() ?? string.Empty);

            if (!entities.Any())
            {
                PushL($"<b>Контакт не обнаружен, попробуй еще раз</b>");
                PushL("");
                PushL(">");
                PushL();

                RowButton("Отмена", Q(MainPanel));

                await State(new WaitEnterFio());

                return;
            }

            if (entities.Count() > 1)
            {
                PushL($"<b>Обнаружено несколько контактов, выбери нужный</b>");
                PushL("");

                if (entities.Count() > 12)
                {
                    PushL("*Обрати внимание! Обнаружено слишком много контактов, попробуй использовтаь более точный запрос");
                    PushL("");

                    entities = entities.Take(12);
                }

                PushL(">");
                PushL();

                int split = 0;
                int splitLimit = 3;

                foreach (var entity in entities)
                {
                    if (split == splitLimit)
                        split = 0;

                    if (split == 0)
                    {
                        RowButton(entity.Fio, Q(ViewContact, entity.Id));
                    }
                    else
                    {
                        Button(entity.Fio, Q(ViewContact, entity.Id));
                    }

                    split++;
                }

                RowButton("Отмена", Q(MainPanel));

                return;
            }

            ViewContact(entities.ElementAt(0).Id);
        }

        [Action]
        public async Task SelectPhone()
        {
            PushL($"<b>Введи телефон для поиска</b>");
            PushL("");
            PushL("Примеры корректного ввода (регистр не учитывается)");
            PushL("");
            PushL("• <i>(3467) 39-20-45, 36-03-60</i>");
            PushL("• <i>(3467) 39-20-45</i>");
            PushL("• <i>39-20-45</i>");
            PushL("• <i>36-03-60</i>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Отмена", Q(MainPanel));

            await State(new WaitEnterPhone());
        }

        [State]
        public async ValueTask EnterPhone(WaitEnterPhone _)
        {
            var entities = _storage.FindBestMatchesByPhone(Context.GetSafeTextPayload() ?? string.Empty);

            if (!entities.Any())
            {
                PushL($"<b>Контакт не обнаружен, попробуй еще раз</b>");
                PushL("");
                PushL(">");
                PushL();

                RowButton("Отмена", Q(MainPanel));

                await State(new WaitEnterPhone());

                return;
            }

            if (entities.Count() > 1)
            {
                PushL($"<b>Обнаружено несколько контактов, выбери нужный</b>");
                PushL("");

                if (entities.Count() > 12)
                {
                    PushL("*Обрати внимание! Обнаружено слишком много контактов, попробуй использовтаь более точный запрос");
                    PushL("");

                    entities = entities.Take(12);
                }

                PushL(">");
                PushL();

                int split = 0;
                int splitLimit = 3;

                foreach (var entity in entities)
                {
                    if (split == splitLimit)
                        split = 0;

                    if (split == 0)
                    {
                        RowButton(entity.Fio, Q(ViewContact, entity.Id));
                    }
                    else
                    {
                        Button(entity.Fio, Q(ViewContact, entity.Id));
                    }

                    split++;
                }

                RowButton("Отмена", Q(MainPanel));

                return;
            }

            ViewContact(entities.ElementAt(0).Id);
        }

        [Action]
        public async Task SelectRoom()
        {
            PushL($"<b>Введи кабинет для поиска</b>");
            PushL("");
            PushL("Примеры корректного ввода (регистр не учитывается)");
            PushL("");
            PushL("• <i>ул.к.маркса, д.14, каб.506</i>");
            PushL("• <i>ул.к.маркса, д.14</i>");
            PushL("• <i>каб.506</i>");
            PushL("• <i>506</i>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Отмена", Q(MainPanel));

            await State(new WaitEnterRoom());
        }

        [State]
        public async ValueTask EnterRoom(WaitEnterRoom _)
        {
            var entities = _storage.FindBestMatchesByRoom(Context.GetSafeTextPayload() ?? string.Empty);

            if (!entities.Any())
            {
                PushL($"<b>Контакт не обнаружен, попробуй еще раз</b>");
                PushL("");
                PushL(">");
                PushL();

                RowButton("Отмена", Q(MainPanel));

                await State(new WaitEnterRoom());

                return;
            }

            if (entities.Count() > 1)
            {
                PushL($"<b>Обнаружено несколько контактов, выбери нужный</b>");
                PushL("");

                if (entities.Count() > 12)
                {
                    PushL("*Обрати внимание! Обнаружено слишком много контактов, попробуй использовтаь более точный запрос");
                    PushL("");

                    entities = entities.Take(12);
                }

                PushL(">");
                PushL();

                int split = 0;
                int splitLimit = 3;

                foreach (var entity in entities)
                {
                    if (split == splitLimit)
                        split = 0;

                    if (split == 0)
                    {
                        RowButton(entity.Fio, Q(ViewContact, entity.Id));
                    }
                    else
                    {
                        Button(entity.Fio, Q(ViewContact, entity.Id));
                    }

                    split++;
                }

                RowButton("Отмена", Q(MainPanel));

                return;
            }

            ViewContact(entities.ElementAt(0).Id);
        }

        [Action]
        public async Task SelectPost()
        {
            PushL($"<b>Введи должность для поиска</b>");
            PushL("");
            PushL("Примеры корректного ввода (регистр не учитывается)");
            PushL("");
            PushL("• <i>Заместитель руководителя Аппарата Губернатора, Правительства</i>");
            PushL("• <i>Помощник</i>");
            PushL("• <i>Начальник управления</i>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Отмена", Q(MainPanel));

            await State(new WaitEnterPost());
        }

        [State]
        public async ValueTask EnterPost(WaitEnterPost _)
        {
            var entities = _storage.FindBestMatchesByPost(Context.GetSafeTextPayload() ?? string.Empty);

            if (!entities.Any())
            {
                PushL($"<b>Контакт не обнаружен, попробуй еще раз</b>");
                PushL("");
                PushL(">");
                PushL();

                RowButton("Отмена", Q(MainPanel));

                await State(new WaitEnterPost());

                return;
            }

            if (entities.Count() > 1)
            {
                PushL($"<b>Обнаружено несколько контактов, выбери нужный</b>");
                PushL("");

                if (entities.Count() > 12)
                {
                    PushL("*Обрати внимание! Обнаружено слишком много контактов, попробуй использовтаь более точный запрос");
                    PushL("");

                    entities = entities.Take(12);
                }

                PushL(">");
                PushL();

                int split = 0;
                int splitLimit = 3;

                foreach (var entity in entities)
                {
                    if (split == splitLimit)
                        split = 0;

                    if (split == 0)
                    {
                        RowButton(entity.Fio, Q(ViewContact, entity.Id));
                    }
                    else
                    {
                        Button(entity.Fio, Q(ViewContact, entity.Id));
                    }

                    split++;
                }

                RowButton("Отмена", Q(MainPanel));

                return;
            }

            ViewContact(entities.ElementAt(0).Id);
        }

        [Action]
        public async Task SelectDynamic()
        {
            PushL($"<b>Введи любые известные данные</b>");
            PushL("");
            PushL("Примеры корректного ввода (регистр не учитывается)");
            PushL("");
            PushL("• <i>уткин</i>");
            PushL("• <i>анатолий</i>");
            PushL("• <i>валерьевич</i>");
            PushL("• <i>анатолий валерьевич</i>");
            PushL("• <i>уткин анатолий валерьевич</i>");
            PushL("• <i>начальник управления</i>");
            PushL("• <i>советник</i>");
            PushL("• <i>(3467) 39-20-80, 36-04-02</i>");
            PushL("• <i>39-20-80</i>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Отмена", Q(MainPanel));

            await State(new WaitEnterDynamic());
        }

        [State]
        public async ValueTask DynamicEnter(WaitEnterDynamic _)
        {
            var entities = _storage.FindBestMatches(Context.GetSafeTextPayload() ?? string.Empty);

            if (!entities.Any())
            {
                PushL($"<b>Контакт не обнаружен, попробуй еще раз</b>");
                PushL("");
                PushL(">");
                PushL();

                RowButton("Отмена", Q(MainPanel));

                await State(new WaitEnterDynamic());

                return;
            }

            if (entities.Count() > 1)
            {
                PushL($"<b>Обнаружено несколько контактов, выбери нужный</b>");
                PushL("");

                if (entities.Count() > 12)
                {
                    PushL("*Обрати внимание! Обнаружено слишком много контактов, попробуй использовтаь более точный запрос");
                    PushL("");

                    entities = entities.Take(12);
                }

                PushL(">");
                PushL();

                int split = 0;
                int splitLimit = 3;

                foreach (var entity in entities)
                {
                    if (split == splitLimit)
                        split = 0;

                    if (split == 0)
                    {
                        RowButton(entity.Fio, Q(ViewContact, entity.Id));
                    }
                    else
                    {
                        Button(entity.Fio, Q(ViewContact, entity.Id));
                    }

                    split++;
                }

                RowButton("Отмена", Q(MainPanel));

                return;
            }

            ViewContact(entities.ElementAt(0).Id);
        }

        [Action]
        public void ViewContact(int entityId)
        {
            var contact = _storage.GetPhoneBook.First(x => x.Id == entityId);

            PushL($"<b>Контакт №{contact.Id} телефонной книги</b>");
            PushL("");
            PushL($"<b>Фио:</b> <i>{contact.Fio}</i>");
            PushL($"<b>Должность:</b> <i>{contact.Post}</i>");
            PushL($"<b>Телефон:</b> <i>{contact.Phone}</i>");
            PushL($"<b>Вн. телефон:</b> <i>{contact.AnotherPhone}</i>");
            PushL($"<b>Кабинет:</b> <i>{contact.Room}</i>");
            PushL($"<b>Почта:</b> <i>{contact.Mail}</i>");
            PushL("");
            PushL(">");
            PushL();

            RowButton("Вернуться", Q(MainPanel));
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
