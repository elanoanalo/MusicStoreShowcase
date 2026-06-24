using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicStoreShowcase.Models
{
    public static class TrackGenerator
    {
        public static List<MusicTrack> GenerateBatch(
            ulong userSeed,
            LocaleDefinition locale,
            double avgLikes,
            int pageNumber,
            int pageSize = 10)
        {
            var tracks = new List<MusicTrack>();
            int startIndex = ((pageNumber - 1) * pageSize) + 1;

            for (int i = 0; i < pageSize; i++)
            {
                int currentTrackIndex = startIndex + i;
                tracks.Add(GenerateSingleTrack(userSeed, locale, avgLikes, currentTrackIndex));
            }

            return tracks;
        }

        private static MusicTrack GenerateSingleTrack(
            ulong userSeed,
            LocaleDefinition locale,
            double avgLikes,
            int trackIndex)
        {

            unchecked
            {
                ulong combined = userSeed * 6364136223846793005UL + (ulong)trackIndex;
                int contentSeed = (int)(combined & 0x7FFFFFFF); 

                var contentFaker = new Faker(locale.BogusLocale)
                {
                    Random = new Randomizer(contentSeed)
                };

                string title = BuildFromPattern(contentFaker, locale.TitlePatterns, locale);
                string artist = BuildArtist(contentFaker, locale);
                string album = BuildAlbum(contentFaker, locale);
                string genre = contentFaker.PickRandom(locale.Genres.ToArray());

                int minutes = contentFaker.Random.Int(1, 5);
                int seconds = contentFaker.Random.Int(0, 59);
                string duration = $"{minutes}:{seconds:D2}";

                string review = locale.ReviewTemplate.Replace("{genre}", genre);

                int likesSeed = contentFaker.Random.Int(int.MinValue, int.MaxValue);
                var likesRandom = new Random(likesSeed);
                int calculatedLikes = CalculateProbabilisticLikes(avgLikes, likesRandom);

                return new MusicTrack
                {
                    Index = trackIndex,
                    Id = Guid.NewGuid().ToString("N").Substring(0, 12),
                    Title = title,
                    Artist = artist,
                    Album = album,
                    Genre = genre,
                    Duration = duration,
                    Review = review,
                    Likes = calculatedLikes
                };
            }
        }

        private static string BuildFromPattern(Faker faker, List<string> patterns, LocaleDefinition locale)
        {
            string pattern = faker.PickRandom(patterns.ToArray());

            string result = pattern;
            result = ReplaceAllOccurrences(result, "{adj}", () => faker.PickRandom(locale.Adjectives.ToArray()));
            result = ReplaceAllOccurrences(result, "{noun}", () => faker.PickRandom(locale.Nouns.ToArray()));

            if (result.Length > 0)
                result = char.ToUpper(result[0]) + result.Substring(1);

            return result;
        }

        private static string ReplaceAllOccurrences(string input, string placeholder, Func<string> valueFactory)
        {
            while (input.Contains(placeholder))
            {
                int index = input.IndexOf(placeholder, StringComparison.Ordinal);
                input = input.Substring(0, index) + valueFactory() + input.Substring(index + placeholder.Length);
            }
            return input;
        }

        private static string BuildArtist(Faker faker, LocaleDefinition locale)
        {
            string pattern = faker.PickRandom(locale.ArtistPatterns.ToArray());

            if (pattern == "fullName")
            {
                return faker.Name.FullName();
            }

            string result = pattern;
            result = ReplaceAllOccurrences(result, "{adj}", () => faker.PickRandom(locale.Adjectives.ToArray()));
            result = ReplaceAllOccurrences(result, "{noun}", () => faker.PickRandom(locale.Nouns.ToArray()));
            result = ReplaceAllOccurrences(result, "{groupSuffix}", () => faker.PickRandom(locale.GroupSuffixes.ToArray()));

            if (result.Length > 0)
                result = char.ToUpper(result[0]) + result.Substring(1);

            return result;
        }

        private static string BuildAlbum(Faker faker, LocaleDefinition locale)
        {
            if (faker.Random.Bool(0.3f))
            {
                return locale.AlbumSingleWord;
            }

            return BuildFromPattern(faker, locale.AlbumPatterns, locale);
        }

        private static int CalculateProbabilisticLikes(double avgLikes, Random rand)
        {
            if (avgLikes < 0) return 0;

            int baseLikes = (int)Math.Floor(avgLikes);
            double remainder = avgLikes % 1;

            if (rand.NextDouble() < remainder)
            {
                baseLikes++;
            }

            return baseLikes;
        }
    }
}