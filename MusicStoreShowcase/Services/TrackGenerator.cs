using Bogus;
using System;
using System.Collections.Generic;

namespace MusicStoreShowcase.Models
{
    public static class TrackGenerator
    {
        private static readonly Dictionary<string, string[]> GenresByLocale = new()
        {
            { "en", new[] { "Rock", "Pop", "Hip-Hop", "Jazz", "Electronic", "House", "Folk", "Heavy Metal" } },
            { "es", new[] { "Rock", "Pop", "Hip-Hop", "Jazz", "Electrónica", "House", "Folk", "Heavy Metal" } }
        };

        // Словарная база для красивого Английского плейлиста
        private static readonly string[] EnAdjectives = { "Midnight", "Golden", "Secret", "Lost", "Silent", "Electric", "Wild", "Dark", "Sweet", "Broken", "Infinite", "velvet", "Neon", "Liquid", "Chasing" };
        private static readonly string[] EnNouns = { "Dream", "Sky", "Heart", "Rain", "Silence", "Shadow", "Fire", "Night", "Ocean", "Memory", "Light", "Echo", "Storm", "Miracle", "Highway" };
        private static readonly string[] EnGroupSuffix = { "Project", "Band", "Club", "Collective", "Chronicles" };

        // Словарная база для красивого Испанского плейлиста
        private static readonly string[] EsAdjectives = { "Secreto", "Perdido", "Eterno", "Luminoso", "Salvaje", "Oscuro", "Dulce", "Roto", "Infinito", "Azul", "Silencioso", "Nocturno" };
        private static readonly string[] EsNouns = { "Sueño", "Cielo", "Corazón", "Lluvia", "Silencio", "Sombra", "Fuego", "Noche", "Océano", "Memoria", "Luz", "Eco", "Camino", "Destino" };
        private static readonly string[] EsGroupSuffix = { "Banda", "Proyecto", "Club", "Colectivo", "Trío" };

        public static List<MusicTrack> GenerateBatch(ulong userSeed, string language, double avgLikes, int pageNumber, int pageSize = 10)
        {
            var tracks = new List<MusicTrack>();
            string locale = (language?.Trim().ToLower() == "es") ? "es" : "en";
            bool isEs = (locale == "es");

            int startIndex = ((pageNumber - 1) * pageSize) + 1;

            for (int i = 0; i < pageSize; i++)
            {
                int currentTrackIndex = startIndex + i;
                int recordSeed = (int)(userSeed ^ (ulong)currentTrackIndex);

                var contentFaker = new Faker(locale);
                contentFaker.Random = new Randomizer(recordSeed);

                string title = "";
                string artistName = "";
                string album = "";

                // Выбираем базу слов в зависимости от языка
                var adjBank = isEs ? EsAdjectives : EnAdjectives;
                var nounBank = isEs ? EsNouns : EnNouns;
                var groupBank = isEs ? EsGroupSuffix : EnGroupSuffix;

                // ==========================================
                // 1. РЕАЛИСТИЧНЫЕ НАЗВАНИЯ ПЕСЕН (1-3 СЛОВА)
                // ==========================================
                int titleStyle = contentFaker.Random.Int(1, 3);
                if (titleStyle == 1)
                {
                    // Одно слово: "Dream" / "Sueño"
                    title = contentFaker.PickRandom(nounBank);
                }
                else if (titleStyle == 2)
                {
                    // Два слова: Прилагательное + Существительное ("Golden Sky" / "Cielo Azul")
                    string adj = contentFaker.PickRandom(adjBank);
                    string noun = contentFaker.PickRandom(nounBank);
                    title = isEs ? $"{noun} {adj}" : $"{adj} {noun}"; // В испанском прилагательное обычно после существительного
                }
                else
                {
                    // Три слова: "Chasing Midnight Rain" / "Luz Del Silencio"
                    if (isEs)
                        title = $"{contentFaker.PickRandom(nounBank)} Del {contentFaker.PickRandom(nounBank)}";
                    else
                        title = $"{contentFaker.PickRandom(adjBank)} {contentFaker.PickRandom(adjBank)} {contentFaker.PickRandom(nounBank)}";
                }
                title = char.ToUpper(title[0]) + title.Substring(1);

                // ==========================================
                // 2. РЕАЛИСТИЧНЫЕ АВТОРЫ (БЕЗ КОРПОРАТИВНОГО МУСОРА)
                // ==========================================
                int artistStyle = contentFaker.Random.Int(1, 3);
                if (artistStyle == 1)
                {
                    // Настоящее имя и фамилия (например: "Diego Torres" или "John Miller")
                    artistName = contentFaker.Name.FullName();
                }
                else if (artistStyle == 2)
                {
                    // Название группы: "Neon Band" / "Proyecto Eterno"
                    string word = contentFaker.PickRandom(nounBank);
                    string suffix = contentFaker.PickRandom(groupBank);
                    artistName = isEs ? $"{suffix} {word}" : $"{word} {suffix}";
                }
                else
                {
                    // Сольный стильный псевдоним из одного слова
                    artistName = contentFaker.PickRandom(adjBank);
                }

                // ==========================================
                // 3. ЧИСТЫЕ АЛЬБОМЫ (БЕЗ СКОБОК И ЗАПЯТЫХ)
                // ==========================================
                if (contentFaker.Random.Bool(0.3f))
                {
                    album = isEs ? "Sencillo" : "Single";
                }
                else
                {
                    // Название альбома — красивое сочетание из 1-2 слов
                    int albumStyle = contentFaker.Random.Int(1, 2);
                    if (albumStyle == 1)
                    {
                        album = contentFaker.PickRandom(nounBank);
                    }
                    else
                    {
                        string adj = contentFaker.PickRandom(adjBank);
                        string noun = contentFaker.PickRandom(nounBank);
                        album = isEs ? $"{noun} {adj}" : $"{adj} {noun}";
                    }
                }

                // ==========================================
                // ОСТАЛЬНЫЕ МЕТАДАННЫЕ ТРЕКА
                // ==========================================
                var genres = GenresByLocale.ContainsKey(locale) ? GenresByLocale[locale] : GenresByLocale["en"];
                string genre = contentFaker.PickRandom(genres);

                int minutes = contentFaker.Random.Int(1, 5);
                int seconds = contentFaker.Random.Int(0, 59);
                string duration = $"{minutes}:{seconds:D2}";

                // Чистый красивый отзыв без системного мусора
                string review = isEs
                    ? $"Una de las mejores canciones de este año en el género {genre}."
                    : $"One of the best tracks of this year in {genre} genre.";

                var likesRandom = new Random(recordSeed);
                int calculatedLikes = CalculateProbabilisticLikes(avgLikes, likesRandom);

                var track = new MusicTrack
                {
                    Index = currentTrackIndex,
                    Id = Guid.NewGuid().ToString("N").Substring(0, 12),
                    Title = title,
                    Artist = artistName,
                    Album = album,
                    Genre = genre,
                    Duration = duration,
                    Review = review,
                    Likes = calculatedLikes
                };

                tracks.Add(track);
            }

            return tracks;
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