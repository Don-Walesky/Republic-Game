namespace Republic.Core.Configuration;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON-backed implementation of <see cref="IConfigurationManager"/>.
/// </summary>
public sealed class JsonConfigurationManager : IConfigurationManager
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    static JsonConfigurationManager()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private string? _lastLoadedPath;

    /// <inheritdoc />
    public RepublicConfiguration Current { get; private set; } = new();

    /// <inheritdoc />
    public RepublicConfiguration Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        var json = File.ReadAllText(fullPath);
        Current = JsonSerializer.Deserialize<RepublicConfiguration>(json, SerializerOptions) ?? new RepublicConfiguration();
        _lastLoadedPath = fullPath;
        return Current;
    }

    /// <inheritdoc />
    public void Save(string path, RepublicConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var json = JsonSerializer.Serialize(configuration, SerializerOptions);
        File.WriteAllText(fullPath, json);
        Current = configuration;
        _lastLoadedPath = fullPath;
    }

    /// <inheritdoc />
    public RepublicConfiguration Reload()
    {
        if (string.IsNullOrWhiteSpace(_lastLoadedPath))
        {
            throw new InvalidOperationException("No configuration file has been loaded yet.");
        }

        return Load(_lastLoadedPath);
    }
}
