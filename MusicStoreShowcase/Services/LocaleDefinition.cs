using System.Collections.Generic;

namespace MusicStoreShowcase.Models
{
    public class LocaleDefinition
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string BogusLocale { get; set; } = "en";

        public List<string> Adjectives { get; set; } = new();
        public List<string> Nouns { get; set; } = new();
        public List<string> GroupSuffixes { get; set; } = new();
        public List<string> Genres { get; set; } = new();

        public string AlbumSingleWord { get; set; } = "Single";
        public List<string> TitlePatterns { get; set; } = new();
        public List<string> AlbumPatterns { get; set; } = new();
        public List<string> ArtistPatterns { get; set; } = new();
        public string ReviewTemplate { get; set; } = string.Empty;

        public List<string> Lyrics { get; set; } = new(); 
        public ReviewPhrases Reviews { get; set; } = new();
    }

    public class ReviewPhrases
    {
        public List<string> Openings { get; set; } = new();
        public List<string> Adjectives { get; set; } = new();
        public List<string> Connectors { get; set; } = new();
        public List<string> Conclusions { get; set; } = new();
    }
}