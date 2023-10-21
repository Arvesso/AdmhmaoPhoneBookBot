using AdmhmaoPhoneBookBot.PhoneBook;
using ArveCore.Json;
using Newtonsoft.Json;

namespace AdmhmaoPhoneBookBot.IOControl
{
    public class InMemoryStorage
    {
        private const double SimilarityValue = 0.41; // Higher - more precisely search

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
        public IEnumerable<Entity> FindBestMatches(string input)
        {
            IEnumerable<Entity> result;

            if (string.IsNullOrEmpty(input))
                return Enumerable.Empty<Entity>();

            result = FindBestMatchesByFio(input);

            if (!result.Any())
                result = FindBestMatchesByPhone(input);
            if (!result.Any())
                result = FindBestMatchesByRoom(input);
            if (!result.Any())
                result = FindBestMatchesByPost(input);

            return result;
        }
        public IEnumerable<Entity> FindBestMatchesByFio(string input)
        {
            var list = GetPhoneBook;

            IEnumerable<Entity> result;

            if (string.IsNullOrEmpty(input))
                return Enumerable.Empty<Entity>();

            input = input.Trim();

            result = list.Where(x => x.Fio.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (!result.Any())
                result = list.Where(x => x.Fio.Contains(input, StringComparison.OrdinalIgnoreCase));
            if (!result.Any())
                result = list.Where(x => x.Fio.CalculateSimilarity(input) > SimilarityValue);

            return result;
        }
        public IEnumerable<Entity> FindBestMatchesByPhone(string input)
        {
            var list = GetPhoneBook;

            IEnumerable<Entity> result;

            if (string.IsNullOrEmpty(input))
                return Enumerable.Empty<Entity>();

            input = input.Trim();

            result = list.Where(x => x.Phone.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (!result.Any())
                result = list.Where(x => x.Phone.Contains(input, StringComparison.OrdinalIgnoreCase));

            return result;
        }
        public IEnumerable<Entity> FindBestMatchesByRoom(string input)
        {
            var list = GetPhoneBook;

            IEnumerable<Entity> result;

            if (string.IsNullOrEmpty(input))
                return Enumerable.Empty<Entity>();

            input = input.Trim();

            result = list.Where(x => x.Room.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (!result.Any())
                result = list.Where(x => x.Room.Contains(input, StringComparison.OrdinalIgnoreCase));
            if (!result.Any())
                result = list.Where(x => x.Room.CalculateSimilarity(input) > SimilarityValue);

            return result;
        }
        public IEnumerable<Entity> FindBestMatchesByPost(string input)
        {
            var list = GetPhoneBook;

            IEnumerable<Entity> result;

            if (string.IsNullOrEmpty(input))
                return Enumerable.Empty<Entity>();

            input = input.Trim();

            result = list.Where(x => x.Post.Equals(input, StringComparison.OrdinalIgnoreCase));

            if (!result.Any())
                result = list.Where(x => x.Post.Contains(input, StringComparison.OrdinalIgnoreCase));
            if (!result.Any())
                result = list.Where(x => x.Post.CalculateSimilarity(input) > SimilarityValue);

            return result;
        }
        public void SerializePhoneBook()
        {
            var content = JsonConvert.SerializeObject(PhoneBook, SerializeSettings.DefaultSettings);
            File.WriteAllText(_phoneBookFilePath, content);
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
