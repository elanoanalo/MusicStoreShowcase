using System;
using System.Collections.Generic;

namespace MusicStoreShowcase.Models
{
    public static class SongTextGenerator
    {
        public static List<string> GenerateReviews(ulong userSeed, int trackIndex, LocaleDefinition locale, int count = 3)
        {
            var reviews = new List<string>();
            int reviewSeed = (int)((userSeed * 1103515245UL + (ulong)trackIndex) & 0x7FFFFFFF);
            var rand = new Random(reviewSeed);

            var openings = (locale?.Reviews?.Openings != null && locale.Reviews.Openings.Count > 0)
                ? locale.Reviews.Openings : new List<string> { "This track is", "In my opinion, it's", "Wow," };

            var adjectives = (locale?.Reviews?.Adjectives != null && locale.Reviews.Adjectives.Count > 0)
                ? locale.Reviews.Adjectives : new List<string> { "incredible", "amazing", "beautiful" };

            var connectors = (locale?.Reviews?.Connectors != null && locale.Reviews.Connectors.Count > 0)
                ? locale.Reviews.Connectors : new List<string> { "and full of", "with intense", "creating a great" };

            var conclusions = (locale?.Reviews?.Conclusions != null && locale.Reviews.Conclusions.Count > 0)
                ? locale.Reviews.Conclusions : new List<string> { "energy.", "vibes.", "atmosphere." };


            int attempts = 0;
            while (reviews.Count < count && attempts < count * 20)
            {
                attempts++;

                string review = $"{openings[rand.Next(openings.Count)]} " +
                                $"{adjectives[rand.Next(adjectives.Count)]} " +
                                $"{connectors[rand.Next(connectors.Count)]} " +
                                $"{conclusions[rand.Next(conclusions.Count)]}";

                if (!reviews.Contains(review))
                    reviews.Add(review);
            }

            return reviews;
        }

        public static List<string> GenerateLyrics(ulong userSeed, int trackIndex, LocaleDefinition locale)
        {
            var lyrics = new List<string>();
            int lyricsSeed = (int)((userSeed * 134775813UL + (ulong)trackIndex) & 0x7FFFFFFF);
            var rand = new Random(lyricsSeed);

            var availableLines = (locale?.Lyrics != null && locale.Lyrics.Count > 0)
                ? locale.Lyrics
                : new List<string> { "La la la, let the music play", "In the night, we find our way", "Feel the rhythm, feel the heat", "Dancing down the neon street" };

            var linesList = new List<string>(availableLines);

            for (int i = 0; i < Math.Min(4, linesList.Count); i++)
            {
                int idx = rand.Next(linesList.Count);
                lyrics.Add(linesList[idx]);
                linesList.RemoveAt(idx); 
            }

            return lyrics;
        }
    }
}