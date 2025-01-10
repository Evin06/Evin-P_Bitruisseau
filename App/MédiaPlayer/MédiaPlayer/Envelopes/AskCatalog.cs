using System.Text.Json;

namespace MédiaPlayer.Envelopes
{
    public class AskCatalog
    {
        public string Content { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
