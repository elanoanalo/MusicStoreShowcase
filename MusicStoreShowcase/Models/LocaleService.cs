using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MusicStoreShowcase.Models
{
    /// <summary>
    /// Загружает все .json файлы из папки локалей один раз при старте
    /// приложения и хранит их в памяти. Это единственное место в проекте,
    /// которое "знает" про конкретные коды языков — и даже оно не хардкодит
    /// их: список языков получается просто перечислением файлов в папке.
    /// </summary>
    public class LocaleService
    {
        private readonly Dictionary<string, LocaleDefinition> _locales = new();

        // Порядок добавления файлов сохраняется — это нужно, чтобы
        // выпадающий список языков в UI показывал их в стабильном порядке
        // (например, по алфавиту имени файла), а не вразнобой.
        private readonly List<string> _orderedCodes = new();

        public LocaleService(string localesFolderPath)
        {
            LoadFromFolder(localesFolderPath);
        }

        private void LoadFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException(
                    $"Locales folder not found: {folderPath}. " +
                    "Make sure wwwroot/locales/*.json files exist and are copied to output.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Сортируем имена файлов по алфавиту, чтобы порядок языков в
            // выпадающем списке был предсказуемым независимо от того, как
            // файловая система отдаёт список файлов.
            var jsonFiles = Directory.GetFiles(folderPath, "*.json")
                                      .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

            foreach (var filePath in jsonFiles)
            {
                string json = File.ReadAllText(filePath);
                var locale = JsonSerializer.Deserialize<LocaleDefinition>(json, options);

                if (locale is null || string.IsNullOrWhiteSpace(locale.Code))
                {
                    // Пропускаем файлы, которые не похожи на валидную локаль,
                    // вместо того чтобы ронять всё приложение из-за одного
                    // битого файла.
                    continue;
                }

                _locales[locale.Code] = locale;
                _orderedCodes.Add(locale.Code);
            }

            if (_locales.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No valid locale JSON files found in {folderPath}.");
            }
        }

        // Список всех доступных локалей в стабильном порядке — используется
        // для построения MudSelect в тулбаре.
        public IReadOnlyList<LocaleDefinition> GetAll() =>
            _orderedCodes.Select(code => _locales[code]).ToList();

        public LocaleDefinition GetByCode(string code)
        {
            if (_locales.TryGetValue(code, out var locale))
                return locale;

            // Если код не найден (например, испорченные данные в URL или
            // старое значение из localStorage), откатываемся на первую
            // доступную локаль, а не бросаем исключение в UI.
            return _locales[_orderedCodes.First()];
        }

        public string DefaultCode => _orderedCodes.First();
    }
}
