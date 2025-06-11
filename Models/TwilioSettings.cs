// Models/TwilioSettings.cs
namespace BirdSing.Models
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = null!;
        public string AuthToken { get; set; } = null!;
        public string FromWhatsapp { get; set; } = null!;
        public string ContentSid { get; set; } = null!;  

        public string FromSms { get; set; } = null!; 
    }
}
