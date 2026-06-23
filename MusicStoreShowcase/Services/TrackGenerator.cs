using Bogus;

namespace MusicStoreShowcase.Models
{
    public static class TrackGenerator
    {
        // Карта локалей для Bogus: интерфейс шлет "en", а Bogus требует "en" или "ru"
        private static readonly Dictionary<string, string> LocaleMap = new()
        {
            { "en", "en" },
            { "ru", "ru" }
        };

        // Список музыкальных жанров (вынесли отдельно, чтобы локализовать)
        private static readonly Dictionary<string, string[]> GenresByLocale = new()
        {
            { "en", new[] { "Rock", "Pop", "Hip-Hop", "Jazz", "Electronic", "House", "Folk", "Heavy Metal" } },
            { "ru", new[] { "Рок", "Поп", "Хип-Хоп", "Джаз", "Электроника", "Хаус", "Фолк", "Хэви-Метал" } }
        };

        public static List<MusicTrack> GenerateBatch(ulong userSeed, string language, double avgLikes, int pageNumber, int pageSize = 10)
        {
            var tracks = new List<MusicTrack>();

            // Определяем локаль для Bogus
            string locale = LocaleMap.TryGetValue(language, out var loc) ? loc : "en";

            // Вычисляем стартовый индекс для этой страницы (например, для стр.1 это 1, для стр.2 это 11)
            int startIndex = ((pageNumber - 1) * pageSize) + 1;

            for (int i = 0; i < pageSize; i++)
            {
                int currentTrackIndex = startIndex + i;

                // ВАЖНО ПО ТЗ: Сид для конкретной записи должен быть комбинацией базового сида и индекса записи!
                // Используем простую MAD-операцию (Умножение-Сложение), чтобы получить уникальный, но детерминированный сид записи.
                int recordSeed = (int)(userSeed ^ (ulong)currentTrackIndex);

                // 1. Создаем генератор для КОРЕЙНОГО контента (Названия, Артисты, Альбомы)
                // Он зависит ТОЛЬКО от recordSeed. Если поменяются лайки, этот генератор выдаст ТЕ ЖЕ САМЫЕ строки!
                var contentFaker = new Faker(locale);
                contentFaker.Random = new Randomizer(recordSeed);

                // Генерируем базовые текстовые данные
                string artistName = contentFaker.Random.Bool(0.4f)
                    ? contentFaker.Company.CompanyName() // 40% — название группы
                    : contentFaker.Name.FullName();      // 60% — имя сольного артиста

                string title = contentFaker.Commerce.ProductName(); // Фейковое название трека (типа "Fast Carrot")

                string album = contentFaker.Random.Bool(0.2f)
                    ? "Single"
                    : contentFaker.Commerce.Department() + " Album";

                // Выбираем жанр из нашего словаря локализации
                var genres = GenresByLocale.ContainsKey(locale) ? GenresByLocale[locale] : GenresByLocale["en"];
                string genre = contentFaker.PickRandom(genres);

                // Генерируем длительность трека (от 1:30 до 5:00)
                int minutes = contentFaker.Random.Int(1, 5);
                int seconds = contentFaker.Random.Int(0, 59);
                string duration = $"{minutes}:{seconds:D2}";

                // Фейковый отзыв
                string review = contentFaker.Lorem.Sentence(6);

                // 2. Алгоритм дробных лайков профессора Лебедева!
                // Инициализируем отдельный генератор для лайков, сидируя его от контентного сида.
                // Это гарантирует, что лайки не сломают структуру, но посчитаются честно.
                var likesRandom = new Random(recordSeed);
                int calculatedLikes = CalculateProbabilisticLikes(avgLikes, likesRandom);

                // Собираем готовый трек
                var track = new MusicTrack
                {
                    Index = currentTrackIndex,
                    Id = Guid.NewGuid().ToString("N").Substring(0, 12), // уникальный хэш
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

        // Перенос JavaScript-функции профессора Лебедева на C# (n % 1 с округлением и вероятностью)
        private static int CalculateProbabilisticLikes(double avgLikes, Random rand)
        {
            if (avgLikes < 0) return 0;

            int baseLikes = (int)Math.Floor(avgLikes); // Целая часть (например, 4 из 4.7)
            double remainder = avgLikes % 1;           // Дробная часть (0.7 из 4.7)

            // Если случайное число (от 0.0 до 1.0) меньше остатка, добавляем +1 лайк
            if (rand.NextDouble() < remainder)
            {
                baseLikes++;
            }

            return baseLikes;
        }
    }
}