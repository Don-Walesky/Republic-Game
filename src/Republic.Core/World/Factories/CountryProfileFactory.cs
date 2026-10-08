namespace Republic.Core.World.Factories;

using System;
using Republic.Core.Economy.Treasury;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Republic.Core.World.Rules;

/// <summary>
/// Seeded factory producing deterministic country profiles and sovereign countries.
/// Uses strict seed-based pseudo-random number generation; never relies on ambient system time or unseeded RNG.
/// </summary>
public sealed class CountryProfileFactory : ICountryProfileFactory
{
    private readonly ICountryNameRule _countryNameRule;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryProfileFactory"/> class.
    /// </summary>
    public CountryProfileFactory(ICountryNameRule? countryNameRule = null)
    {
        _countryNameRule = countryNameRule ?? CountryNameRule.Default;
    }

    private static readonly string[] Names =
    {
        "Republic of Veridia",
        "Republic of Solaria",
        "Federal Republic of Aethel",
        "Caldera Republic",
        "Republic of Oakhaven",
        "Republic of Valerius",
        "Kaelen Republic",
        "Democratic Republic of Thalassia",
        "Zephyria Republic",
        "Republic of Maridia",
        "Republic of Boreal",
        "Republic of Corvallis"
    };

    private static readonly string[] Capitals =
    {
        "Veridiana",
        "Solaria Prime",
        "Aethelgard",
        "Caldera City",
        "Oakport",
        "Valeria",
        "Kaelen Harbor",
        "Thalassa",
        "Zephyr Point",
        "Maridian Citadel",
        "Borealis",
        "Corvallis Center"
    };

    private static readonly string[] Languages =
    {
        "English",
        "French",
        "Arcadian",
        "Spanish",
        "German",
        "Solaric",
        "Aethelic",
        "Portuguese",
        "Dutch",
        "Swahili"
    };

    private static readonly string[] Constitutions =
    {
        "Charter of Fundamental Liberties",
        "Constitutional Act of Liberation",
        "Federal Charter of Rights",
        "Supreme Covenant of the Sovereign State",
        "National Democratic Constitution",
        "Basic Law of the Federation"
    };

    private static readonly string[] Resources =
    {
        "Iron",
        "Petroleum",
        "Bauxite",
        "Natural Gas",
        "Copper",
        "Rare Earths",
        "Timber",
        "Uranium",
        "Gold",
        "Lithium"
    };

    private static readonly string[] PoliticalCultures =
    {
        "A pragmatic coalition-driven democracy emphasizing civic duty and institutional stability.",
        "A centralized republican tradition founded on national solidarity and statutory civil liberties.",
        "A decentralized federal commonwealth prizing regional autonomy and competitive statecraft.",
        "A disciplined civic society rooted in constitutional jurisprudence and meritocratic administration.",
        "A vibrant political ecosystem driven by civic activism, trade consensus, and open governance.",
        "A maritime trading republic valuing mercantile freedom and strong legal oversight."
    };

    /// <inheritdoc />
    public CountryProfile CreateProfile(int seed, RepublicTime foundingTime = default)
    {
        var rng = new Random(seed);

        var candidateName = Names[rng.Next(Names.Length)];
        // Factory must call the existing CountryName rule. A generated name that fails is a test failure, not a silent rewrite.
        var validatedName = _countryNameRule.Validate(candidateName);

        var capital = Capitals[rng.Next(Capitals.Length)];
        var region = (CountryRegion)rng.Next(0, 5);
        var population = (long)rng.Next(1_500_000, 65_000_000);
        var language = Languages[rng.Next(Languages.Length)];
        var govForm = (GovernmentForm)rng.Next(0, 4);
        var constitution = Constitutions[rng.Next(Constitutions.Length)];
        var resource = Resources[rng.Next(Resources.Length)];
        var culture = PoliticalCultures[rng.Next(PoliticalCultures.Length)];

        return new CountryProfile(
            officialName: validatedName,
            capital: capital,
            region: region,
            population: population,
            dominantLanguage: language,
            governmentForm: govForm,
            constitutionName: constitution,
            primaryResource: resource,
            politicalCulture: culture,
            foundingTime: foundingTime);
    }

    /// <inheritdoc />
    public Country CreateCountry(int seed, RepublicTime foundingTime = default, string? id = null, double startingTreasury = 1_000_000.0)
    {
        var profile = CreateProfile(seed, foundingTime);
        var countryId = id ?? $"country-{seed:x8}";

        return Country.Found(
            name: profile.OfficialName,
            foundingTime: foundingTime,
            id: countryId,
            capitalCity: profile.Capital,
            governmentType: profile.GovernmentForm.ToString(),
            treasury: new RepuTreasury(startingTreasury, countryId),
            profile: profile);
    }
}
