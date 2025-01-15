namespace M�diaPlayer.Models
{
    public class Music
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }
        public string Extension { get; set; }
        public string FileType { get; set; }
        public long Size { get; set; } // Taille du fichier en octets

        public string GetFormattedDetails()
        {
            return $"{Title} | {Artist} | {FileType} | {Size} bytes | Dur�e : {Duration}";
        }
    }
}
