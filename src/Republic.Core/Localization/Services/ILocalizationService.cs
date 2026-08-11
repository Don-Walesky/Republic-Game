namespace Republic.Core.Localization.Services;

using Republic.Core.Localization.Models;

/// <summary>
/// Service interface providing multilingual string table lookups and language switches.
/// </summary>
public interface ILocalizationService
{
    Language CurrentLanguage { get; set; }
    string GetText(string key);
    string GetText(string key, params object[] args);
    void RegisterTranslation(Language language, string key, string translation);
}
