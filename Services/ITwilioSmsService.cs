using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirdSing.Services
{
   
    public interface ITwilioService
    {
        /// <param name="toWhatsapp">
        /// Número destino con prefijo "whatsapp:+52..."
        /// </param>
        /// <param name="templateVariables">
        /// Lista de valores para {{1}}, {{2}}, … de la plantilla
        /// </param>
        Task SendWhatsappAsync(string toWhatsapp, List<string> templateVariables);
    }
}
