using System.Collections.Generic;

namespace MusicStoreShowcase.Models
{
    /// <summary>
    /// Описывает один язык/регион целиком. Один файл JSON в wwwroot/locales/
    /// десериализуется в один объект этого класса. Никакой логики тут нет —
    /// только данные. Чтобы добавить новый язык, достаточно положить новый
    /// .json файл с такой структурой — менять C#-код не нужно.
    /// </summary>
    public class LocaleDefinition
    {
        // Короткий код языка, используется как ключ ("en", "es", "de"...).
        public string Code { get; set; } = string.Empty;

        // То, что увидит пользователь в выпадающем списке ("English (US)").
        public string DisplayName { get; set; } = string.Empty;

        // Какую локаль использовать в библиотеке Bogus для не-музыкальных
        // полей (например, ФИО исполнителя через contentFaker.Name.FullName()).
        // Не обязательно совпадает с Code: например, для условного
        // "Ukrainian (Ukraine)" Bogus может не иметь точного аналога,
        // и тогда здесь можно указать наиболее близкую доступную локаль Bogus.
        public string BogusLocale { get; set; } = "en";

        // Банки слов, из которых собираются названия и исполнители.
        public List<string> Adjectives { get; set; } = new();
        public List<string> Nouns { get; set; } = new();
        public List<string> GroupSuffixes { get; set; } = new();

        // Список жанров на этом языке.
        public List<string> Genres { get; set; } = new();

        // Буквальная строка для альбома-сингла ("Single" / "Sencillo").
        public string AlbumSingleWord { get; set; } = "Single";

        // Шаблоны названий песни. Каждый шаблон — строка с местозаполнителями
        // {adj} и {noun}, которые генератор заменит случайным словом из
        // соответствующего банка. Порядок слов внутри шаблона уже учитывает
        // грамматику языка (например, в испанском прилагательное обычно
        // ставится после существительного — это видно из самого шаблона,
        // а не из if-проверки в коде).
        public List<string> TitlePatterns { get; set; } = new();

        // Шаблоны названия альбома (когда это не "Single").
        public List<string> AlbumPatterns { get; set; } = new();

        // Шаблоны имени исполнителя. Специальное значение "fullName" означает
        // "использовать Faker.Name.FullName() для текущей BogusLocale", а не
        // строку-шаблон со словами.
        public List<string> ArtistPatterns { get; set; } = new();

        // Шаблон отзыва. Места {genre} заменяется реальным жанром трека.
        public string ReviewTemplate { get; set; } = string.Empty;
    }
}
