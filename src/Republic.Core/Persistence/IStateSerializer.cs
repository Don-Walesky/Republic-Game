namespace Republic.Core.Persistence;

/// <summary>
/// Serializes and deserializes save envelopes.
/// </summary>
public interface IStateSerializer
{
    /// <summary>
    /// Serializes the supplied envelope.
    /// </summary>
    string Serialize<TState>(SaveEnvelope<TState> envelope);

    /// <summary>
    /// Deserializes the supplied JSON payload.
    /// </summary>
    SaveEnvelope<TState> Deserialize<TState>(string payload);
}
