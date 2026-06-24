using System.Text.Json;

namespace MusicStoreShowcase.Models
{
    public class LocaleService
    {
        private readonly Dictionary<string, LocaleDefinition> _locales = new();

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

            var jsonFiles = Directory.GetFiles(folderPath, "*.json")
                                      .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

            foreach (var filePath in jsonFiles)
            {
                string json = File.ReadAllText(filePath);
                var locale = JsonSerializer.Deserialize<LocaleDefinition>(json, options);

                if (locale is null || string.IsNullOrWhiteSpace(locale.Code))
                {

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

        public IReadOnlyList<LocaleDefinition> GetAll() =>
            _orderedCodes.Select(code => _locales[code]).ToList();

        public LocaleDefinition GetByCode(string code)
        {
            if (_locales.TryGetValue(code, out var locale))
                return locale;

            return _locales[_orderedCodes.First()];
        }

        public string DefaultCode => _orderedCodes.First();
    }
}
