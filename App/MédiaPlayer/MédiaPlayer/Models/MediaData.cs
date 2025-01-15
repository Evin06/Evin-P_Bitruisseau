namespace MédiaPlayer.Models
{
    // Définit les détails des fichiers média stockés ou échangés.
    public class MediaData
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public string Duration { get; set; }


        public string GetFormattedDetails()
        {
            return $"{Title} | {Artist} | {Type} | {Size} bytes | Durée: {Duration}";
        }
    }
  

    // Utilisée pour demander un fichier spécifique par son nom.
    public class FileRequest
    {
        public string Title { get; set; }
    }
}
