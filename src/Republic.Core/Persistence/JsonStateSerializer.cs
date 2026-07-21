namespace Republic.Core.Persistence;

using System.Text.Json;

/// <summary>
/// JSON serializer for persisted state.
/// </summary>
public sealed class JsonStateSerializer : IStateSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <inheritdoc />
    public string Serialize<TState>(SaveEnvelope<TState> envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        return JsonSerializer.Serialize(envelope, SerializerOptions);
    }

    /// <inheritdoc />
    public SaveEnvelope<TState> Deserialize<TState>(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        return JsonSerializer.Deserialize<SaveEnvelope<TState>>(payload, SerializerOptions)
            ?? throw new InvalidOperationException("Unable to deserialize save payload.");
    }
}
