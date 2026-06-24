namespace MusicStoreShowcase.Models
{
    public class MusicTrack
    {
        public int Index { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Likes { get; set; }

        public string Duration { get; set; } = "3:00"; 
        public string Review { get; set; } = string.Empty; 

        public string AudioUrl { get; set; } = string.Empty;
    }
}