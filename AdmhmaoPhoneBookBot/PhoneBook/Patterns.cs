namespace AdmhmaoPhoneBookBot.PhoneBook
{
    public class Patterns
    {
        public static string Phone { get; } = "тел";
        public static List<string> AnotherPhone { get; } = new() { "вн", Phone };
        public static string Room { get; } = "кабинет";
        public static string Mail { get; } = "mail";
        public static string Nbsp { get; } = "&nbsp;";
    }
}
