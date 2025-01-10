// SendCatalog.cs
using MédiaPlayer.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace MédiaPlayer.Envelopes
{
    public class SendCatalog
    {
       
            public List<MediaData> Content { get; set; }
        


        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
