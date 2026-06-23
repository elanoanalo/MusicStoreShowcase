namespace MusicStoreShowcase.Models
{
    public class MusicTrack
    {
        // Базовые поля для таблицы (По ТЗ)
        public int Index { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Likes { get; set; }

        // Поля для раскрывающегося списка (Expanded View)
        public string Duration { get; set; } = "3:00"; // Длительность (например, 2:12)
        public string Review { get; set; } = string.Empty; // Случайный отзыв к треку

        // Для проигрывания музыки нам понадобится аудио-ссылка или массив байт
        // (Мы сгенерируем звук программно, чтобы соблюсти требованию детерминированности по Сиду)
        public string AudioUrl { get; set; } = string.Empty;
    }
}