using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirdSing.Services
{
    public interface ITwilioService
    {
        Task SendWhatsappAsync(string toWhatsapp, Dictionary<string, string> templateVariables);
    }
}
