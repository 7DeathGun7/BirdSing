namespace BirdSing.Models
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = null!;
        public string AuthToken { get; set; } = null!;
        public string FromWhatsapp { get; set; } = null!; // Ej: "whatsapp:+521234567890"
    }
}