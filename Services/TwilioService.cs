using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using BirdSing.Models;
using System.Collections.Generic;
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

        public Task SendWhatsappAsync(string toWhatsapp, Dictionary<string, string> templateVariables)
        {
            var opts = new CreateMessageOptions(new PhoneNumber(toWhatsapp))
            {
                From = new PhoneNumber(_settings.FromWhatsapp),
                ContentSid = _settings.ContentSid,
                ContentVariables = JsonConvert.SerializeObject(templateVariables)
            };
            return MessageResource.CreateAsync(opts);
        }
    }
}
