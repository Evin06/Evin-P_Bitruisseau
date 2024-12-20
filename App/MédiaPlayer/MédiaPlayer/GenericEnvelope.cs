// GenericEnvelope.cs
using System.Text.Json;

namespace MédiaPlayer.Envelopes
{
    public class GenericEnvelope
    {
        public string SenderId { get; set; }
        public MessageType MessageType { get; set; }
        public string EnveloppeJson { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public enum MessageType
    {
        ENVOIE_CATALOGUE,
        DEMANDE_CATALOGUE,
        ENVOIE_FICHIER,
        DEMANDE_FICHIER
    }
}
