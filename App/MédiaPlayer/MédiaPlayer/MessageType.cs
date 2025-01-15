using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MédiaPlayer.Models
{
    namespace MédiaPlayer.Envelopes
    {
        public enum MessageType
        {
            ENVOIE_CATALOGUE,
            DEMANDE_CATALOGUE,
            ENVOIE_FICHIER,
            DEMANDE_FICHIER
        }
    }


}
