namespace AdmhmaoPhoneBookBot.PhoneBook
{
    public class PhoneBookModel
    {
        public List<Entity> PhoneBook { get; set; } = new();
    }

    public class Entity
    {
        public int Id { get; set; } = 0;
        public string Fio { get; set; } = string.Empty;
        public string Post { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AnotherPhone { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
    }
}
