using System.Threading.Tasks;

namespace BirdSing.Services
{
    public interface ITwilioService
    {
        Task SendWhatsappAsync(string toWhatsapp, string message);
    }
}