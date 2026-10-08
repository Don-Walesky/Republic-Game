namespace Republic.Core.World.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Time;
using Republic.Core.World.Events;
using Republic.Core.World.Models;
using Republic.Core.World.Rules;

/// <summary>
/// Service implementation for sovereign nation founding, registry, and stability management.
/// Integrates with the authoritative Republic simulation clock to record canonical founding moments.
/// </summary>
public sealed class CountryService : ICountryService
{
    private readonly List<Country> _countries = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly IRepublicClock _clock;
    private readonly ICountryNameRule _countryNameRule;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryService"/> class.
    /// </summary>
    public CountryService(IEventBus eventBus, ILogger? logger = null, IRepublicClock? clock = null, ICountryNameRule? countryNameRule = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
        _clock = clock ?? RepublicClock.CreateControlled();
        _countryNameRule = countryNameRule ?? CountryNameRule.Default;
    }

    /// <inheritdoc />
    public IRepublicClock Clock => _clock;

    /// <inheritdoc />
    public Country FoundCountry(string name, string capitalCity = "", string governmentType = "Democratic Republic", string id = "")
    {
        var validatedName = _countryNameRule.Validate(name);

        var country = Country.Found(
            name: validatedName,
            foundingTime: _clock.CurrentTime,
            id: id,
            capitalCity: capitalCity,
            governmentType: governmentType);

        return RegisterCountry(country);
    }

    /// <inheritdoc />
    public Country RegisterCountry(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        if (!country.IsFounded)
        {
            country.EstablishFounding(_clock.CurrentTime);
        }

        lock (_lock)
        {
            var existingIndex = _countries.FindIndex(c => c.Id == country.Id);
            if (existingIndex >= 0)
            {
                _countries[existingIndex] = country;
            }
            else
            {
                _countries.Add(country);
            }
        }

        _logger?.LogInfo($"Country registered: '{country.Name}' (ID: {country.Id}, Founded: {country.FoundingTime}, Status: {country.FoundingStatus})");
        _eventBus.PublishAsync(new CountryCreatedEvent(country, country.FoundingTime.WatTimestamp));
        return country;
    }

    /// <inheritdoc />
    public Country? GetCountry(string id)
    {
        lock (_lock)
        {
            return _countries.FirstOrDefault(c => c.Id == id);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<Country> GetAllCountries()
    {
        lock (_lock)
        {
            return _countries.ToList().AsReadOnly();
        }
    }

    /// <inheritdoc />
    public bool UpdateStability(string countryId, double delta)
    {
        lock (_lock)
        {
            var country = _countries.FirstOrDefault(c => c.Id == countryId);
            if (country == null)
            {
                return false;
            }

            country.BaselineStability = Math.Clamp(country.BaselineStability + delta, 0.0, 100.0);
            _logger?.LogInfo($"Country '{country.Name}' stability updated: {country.BaselineStability:0.0}");
            _eventBus.PublishAsync(new StabilityChangedEvent(country.Id, country.BaselineStability, DateTimeOffset.UtcNow));
            return true;
        }
    }
}
