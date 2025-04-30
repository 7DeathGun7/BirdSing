using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using BirdSing.Models;
using System.Threading.Tasks;

namespace BirdSing.Services
{
    public class TwilioService : ITwilioService
    {
        private readonly TwilioSettings _settings;

        public TwilioService(IOptions<TwilioSettings> options)
        {
            _settings = options.Value;
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        }

        public Task SendWhatsappAsync(string toWhatsapp, string message)
        {
            return MessageResource.CreateAsync(
                from: new Twilio.Types.PhoneNumber(_settings.FromWhatsapp),
                to: new Twilio.Types.PhoneNumber(toWhatsapp),
                body: message
            );
        }
    }
}
