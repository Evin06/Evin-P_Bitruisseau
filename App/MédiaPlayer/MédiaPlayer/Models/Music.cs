namespace MédiaPlayer.Models
{
    public class Music
    {
        public string Name { get; set; }  // Nom du fichier
        public string Extension { get; set; }  // Extension du fichier (ex: .mp3)
        public string Duration { get; set; }  // Durée du fichier en minutes et secondes

        // Retourne une chaîne formatée pour affichage dans la ListBox
        public string GetFormattedDetails()
        {
            return $"{Name} | {Extension} | {Duration}";
        }
    }
}
