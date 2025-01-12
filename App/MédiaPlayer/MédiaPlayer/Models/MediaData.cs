namespace MédiaPlayer.Models
{
    // Définit les détails des fichiers média stockés ou échangés.
    public class MediaData
    {
        public string FileName { get; set; }
        public string FileArtist { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string FileDuration { get; set; }


        public string GetFormattedDetails()
        {
            return $"{FileName} | {FileArtist} | {FileType} | {FileSize} bytes | Durée: {FileDuration}";
        }
    }
  

    // Utilisée pour demander un fichier spécifique par son nom.
    public class FileRequest
    {
        public string FileName { get; set; }
    }
}
