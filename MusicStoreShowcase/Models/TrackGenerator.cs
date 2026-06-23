using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicStoreShowcase.Models
{
    public static class TrackGenerator
    {
        /// <summary>
        /// Генерирует один "батч" (страницу) треков. Вся локально-специфичная
        /// информация (слова, жанры, грамматические шаблоны) приходит
        /// исключительно через параметр locale — никакого if/else по коду
        /// языка здесь нет, поэтому добавление нового языка не требует
        /// изменений в этом методе.
        /// </summary>
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
            // === КОМБИНАЦИЯ СИДА И ИНДЕКСА (MAD-подход) ===
            // Простая MAD-операция (Multiply-Add), как и предлагает автор
            // задания: умножаем seed на большое нечётное число, добавляем
            // индекс трека. Благодаря этому соседние индексы дают совсем
            // разные seed-ы (а не отличаются на 1, как при чистом сложении),
            // и при этом результат остаётся детерминированным:
            // тот же userSeed + тот же trackIndex => тот же contentSeed всегда.
            unchecked
            {
                ulong combined = userSeed * 6364136223846793005UL + (ulong)trackIndex;
                int contentSeed = (int)(combined & 0x7FFFFFFF); // держим в неотрицательном диапазоне int

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

                // === ОТДЕЛЬНЫЙ ГЕНЕРАТОР ДЛЯ ЛАЙКОВ ===
                // По примеру автора задания: генератор лайков заседовается
                // ОТ выхода контент-генератора, а не от того же contentSeed
                // напрямую. Так лайки технически "произошли" от контента
                // этого трека, но сами по себе не влияют на то, что
                // contentFaker выдаст дальше (мы берём число один раз,
                // после того как контент уже полностью сгенерирован).
                // Randomizer (тип contentFaker.Random в Bogus) не имеет
                // метода Next(), поэтому берём случайное int через Int().
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

        /// <summary>
        /// Собирает строку по случайно выбранному шаблону из списка.
        /// Шаблон — это строка вида "{adj} {noun}", где местозаполнители
        /// заменяются случайным словом из соответствующего банка слов
        /// локали. Один и тот же метод одинаково работает для любого
        /// языка — вся разница (порядок слов, какие шаблоны вообще
        /// существуют) приходит из locale.TitlePatterns / AlbumPatterns.
        /// </summary>
        private static string BuildFromPattern(Faker faker, List<string> patterns, LocaleDefinition locale)
        {
            string pattern = faker.PickRandom(patterns.ToArray());

            string result = pattern;
            result = ReplaceAllOccurrences(result, "{adj}", () => faker.PickRandom(locale.Adjectives.ToArray()));
            result = ReplaceAllOccurrences(result, "{noun}", () => faker.PickRandom(locale.Nouns.ToArray()));

            // Делаем первую букву заглавной, чтобы названия смотрелись
            // как настоящие заголовки, даже если шаблон начинается со
            // служебного слова (например "Del" в испанском).
            if (result.Length > 0)
                result = char.ToUpper(result[0]) + result.Substring(1);

            return result;
        }

        /// <summary>
        /// Заменяет каждое вхождение placeholder в строке на НОВОЕ случайное
        /// слово (а не одно и то же слово везде) — например, шаблон
        /// "{adj} {adj} {noun}" должен дать два РАЗНЫХ прилагательных.
        /// string.Replace не подходит, т.к. он заменил бы оба {adj} на
        /// одно и то же значение.
        /// </summary>
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

            // "fullName" — зарезервированное значение: значит "не собирать
            // по шаблону слов, а использовать встроенный генератор имён
            // Bogus для текущей BogusLocale".
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
            // 30% треков — синглы (буквальная строка "Single"/"Sencillo"
            // приходит из locale.AlbumSingleWord, а не зашита в коде).
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