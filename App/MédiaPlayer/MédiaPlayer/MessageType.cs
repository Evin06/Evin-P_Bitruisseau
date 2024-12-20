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
            ENVOIE_CATALOGUE = 0,
            DEMANDE_CATALOGUE = 1,
            DEMANDE_FICHIER = 2,
            ENVOIE_FICHIER = 3
        }
    }


}
