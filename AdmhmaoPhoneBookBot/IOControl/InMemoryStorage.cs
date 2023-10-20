using AdmhmaoPhoneBookBot.PhoneBook;
using ArveCore.Json;
using Newtonsoft.Json;

namespace AdmhmaoPhoneBookBot.IOControl
{
    public class InMemoryStorage
    {
        private readonly string _phoneBookFolder;
        private readonly string _phoneBookFileName;
        private readonly string _phoneBookFilePath;
        public InMemoryStorage()
        {
            _phoneBookFolder = "PhoneBook";
            _phoneBookFileName = "PhoneBook.json";
            _phoneBookFilePath = Path.Combine(_phoneBookFolder, _phoneBookFileName);

            Directory.CreateDirectory(_phoneBookFolder);
        }

        private PhoneBookModel PhoneBook { get; init; } = new();
        public List<Entity> GetPhoneBook => PhoneBook.PhoneBook;

        public void AddEntityToPhoneBook(Entity entity) => PhoneBook.PhoneBook.Add(entity);
        public void ClearPhoneBook() => PhoneBook.PhoneBook.Clear();
        public void SerializePhoneBook()
        {
            var content = JsonConvert.SerializeObject(PhoneBook, SerializeSettings.DefaultSettings);
            File.WriteAllText(content, _phoneBookFilePath);
        }
        public void LoadPhoneBook()
        {
            if (!File.Exists(_phoneBookFilePath))
                return;

            try
            {
                var phoneBook = JsonConvert.DeserializeObject<PhoneBookModel>(File.ReadAllText(_phoneBookFilePath))?.PhoneBook;

                if (phoneBook is null)
                    return;

                foreach (var entity in phoneBook)
                {
                    AddEntityToPhoneBook(entity);
                }
            }
            catch { }
        }
    }
}
