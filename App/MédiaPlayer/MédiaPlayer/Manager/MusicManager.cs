using System;
using System.Collections.Generic;
using System.Text.Json;
using MédiaPlayer.Models;  // Ajout du namespace contenant la classe Music

namespace MédiaPlayer
{
    public class MusicManager
    {
        // Sérialiser une liste de musiques en JSON
        public static string SerializeMusicList(List<Music> musicList)
        {
            return JsonSerializer.Serialize(musicList);
        }

        // Désérialiser une liste de musiques depuis JSON
        public static List<Music> DeserializeMusicList(string json)
        {
            return JsonSerializer.Deserialize<List<Music>>(json);
        }
    }
}
