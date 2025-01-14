﻿using MédiaPlayer.Models;
using System.Text.Json;

namespace MédiaPlayer.Envelopes
{
    public class SendMusic
    {
        public string Content { get; set; }
        public MediaData FileInfo { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
