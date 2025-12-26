namespace HadiyahServices.Options
{
    public class EmailSettings
    {
        public string Sender { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
    }
}
