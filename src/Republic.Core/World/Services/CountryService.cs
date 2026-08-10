namespace Republic.Core.World.Services;

using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.World.Events;
using Republic.Core.World.Models;

/// <summary>
/// Service implementation for nation entity management and stability mechanics.
/// </summary>
public sealed class CountryService : ICountryService
{
    private readonly List<Country> _countries = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger? _logger;
    private readonly object _lock = new();

    public CountryService(IEventBus eventBus, ILogger? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public Country RegisterCountry(Country country)
    {
        ArgumentNullException.ThrowIfNull(country);
        lock (_lock)
        {
            _countries.Add(country);
        }

        _logger?.LogInfo($"Country registered: '{country.Name}' (Capital: {country.CapitalCity}, Government: {country.GovernmentType})");
        _eventBus.PublishAsync(new CountryCreatedEvent(country, DateTimeOffset.UtcNow));
        return country;
    }

    public Country? GetCountry(string id)
    {
        lock (_lock)
        {
            return _countries.FirstOrDefault(c => c.Id == id);
        }
    }

    public IReadOnlyList<Country> GetAllCountries()
    {
        lock (_lock)
        {
            return _countries.ToList().AsReadOnly();
        }
    }

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
