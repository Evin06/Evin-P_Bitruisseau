namespace MédiaPlayer.Models
{
    public class Music
    {
        public string Name { get; set; }
        public string Duration { get; set; }
        public string Extension { get; set; }

        public string GetFormattedDetails()
        {
            return $"{Name} | {Extension} | {Duration}";
        }
    }
}
