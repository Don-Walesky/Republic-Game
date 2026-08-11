namespace Republic.Core.Localization.Services;

using System;
using System.Collections.Generic;
using Republic.Core.Localization.Models;

/// <summary>
/// Service implementation providing in-memory multilingual translation lookups.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private readonly Dictionary<Language, Dictionary<string, string>> _tables = new();
    private readonly object _lock = new();

    public Language CurrentLanguage { get; set; } = Language.English;

    public LocalizationService()
    {
        InitializeDefaultTranslations();
    }

    private void InitializeDefaultTranslations()
    {
        // English
        RegisterTranslation(Language.English, "app_title", "Republic - Presidential Desk");
        RegisterTranslation(Language.English, "defcon_alert", "ALERT: DEFCON Level {0}");
        RegisterTranslation(Language.English, "treasury_label", "Treasury Balance: ${0:N0}");

        // French
        RegisterTranslation(Language.French, "app_title", "République - Bureau Présidentiel");
        RegisterTranslation(Language.French, "defcon_alert", "ALERTE: Niveau DEFCON {0}");
        RegisterTranslation(Language.French, "treasury_label", "Solde du Trésor: ${0:N0}");

        // German
        RegisterTranslation(Language.German, "app_title", "Republik - Präsidentenschreibtisch");
        RegisterTranslation(Language.German, "defcon_alert", "WARNUNG: DEFCON Stufe {0}");
        RegisterTranslation(Language.German, "treasury_label", "Staatskasse: ${0:N0}");

        // Spanish
        RegisterTranslation(Language.Spanish, "app_title", "República - Escritorio Presidencial");
        RegisterTranslation(Language.Spanish, "defcon_alert", "ALERTA: Nivel DEFCON {0}");
        RegisterTranslation(Language.Spanish, "treasury_label", "Balance del Tesoro: ${0:N0}");

        // Mandarin
        RegisterTranslation(Language.Mandarin, "app_title", "共和国 - 总统办公桌");
        RegisterTranslation(Language.Mandarin, "defcon_alert", "警报：DEFCON 级别 {0}");
        RegisterTranslation(Language.Mandarin, "treasury_label", "国库余额: ${0:N0}");
    }

    public void RegisterTranslation(Language language, string key, string translation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(translation);

        lock (_lock)
        {
            if (!_tables.TryGetValue(language, out var table))
            {
                table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _tables[language] = table;
            }
            table[key] = translation;
        }
    }

    public string GetText(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        lock (_lock)
        {
            if (_tables.TryGetValue(CurrentLanguage, out var table) && table.TryGetValue(key, out var text))
            {
                return text;
            }

            // Fallback to English if translation is missing
            if (CurrentLanguage != Language.English && _tables.TryGetValue(Language.English, out var engTable) && engTable.TryGetValue(key, out var engText))
            {
                return engText;
            }

            return key;
        }
    }

    public string GetText(string key, params object[] args)
    {
        string raw = GetText(key);
        return string.Format(raw, args);
    }
}
