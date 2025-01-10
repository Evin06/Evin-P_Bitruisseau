// MediaData.cs
namespace MédiaPlayer.Models
{
    public class MediaData
    {
        public string FileName { get; set; }
        public string FileArtist { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string FileDuration { get; set; }

    }
    public class FileRequest
    {
        public string FileName { get; set; }
    }
}
