namespace Republic.Core.Politics;

using System;
using System.Text.Json.Serialization;
using Republic.Core.Time;

/// <summary>
/// Domain model representing a democratic national election held between two sovereign political parties.
/// Holds the country identifier, authoritative clock boundary, winning and losing parties, and their support levels.
/// </summary>
public sealed record CountryElection
{
    /// <summary>
    /// Gets the identifier of the sovereign country where the election was held.
    /// </summary>
    public string CountryId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Republic simulation clock boundary at which the election was conducted.
    /// </summary>
    public RepublicTime Boundary { get; init; }

    /// <summary>
    /// Alias for <see cref="Boundary"/> indicating the clock boundary when the election was held.
    /// </summary>
    [JsonIgnore]
    public RepublicTime HeldBoundary => Boundary;

    /// <summary>
    /// Gets the name of the winning political party that assumes or retains the ruling mandate.
    /// </summary>
    public string WinnerPartyName { get; init; } = string.Empty;

    /// <summary>
    /// Alias for <see cref="WinnerPartyName"/>.
    /// </summary>
    [JsonIgnore]
    public string Winner => WinnerPartyName;

    /// <summary>
    /// Gets the name of the defeated political party that assumes or remains in the opposition.
    /// </summary>
    public string LoserPartyName { get; init; } = string.Empty;

    /// <summary>
    /// Alias for <see cref="LoserPartyName"/>.
    /// </summary>
    [JsonIgnore]
    public string Loser => LoserPartyName;

    /// <summary>
    /// Gets the public support rating of the winning party at the time of the election.
    /// </summary>
    public double WinnerSupport { get; init; }

    /// <summary>
    /// Gets the public support rating of the defeated party at the time of the election.
    /// </summary>
    public double LoserSupport { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryElection"/> record with default values.
    /// </summary>
    public CountryElection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryElection"/> record with validated election results.
    /// </summary>
    /// <param name="countryId">The owning country identifier.</param>
    /// <param name="boundary">The clock boundary at which the election was held.</param>
    /// <param name="winnerPartyName">The name of the winning party.</param>
    /// <param name="loserPartyName">The name of the defeated party.</param>
    /// <param name="winnerSupport">The support rating of the winner.</param>
    /// <param name="loserSupport">The support rating of the loser.</param>
    public CountryElection(
        string countryId,
        RepublicTime boundary,
        string winnerPartyName,
        string loserPartyName,
        double winnerSupport,
        double loserSupport)
    {
        CountryId = countryId ?? string.Empty;
        Boundary = boundary;
        WinnerPartyName = winnerPartyName?.Trim() ?? string.Empty;
        LoserPartyName = loserPartyName?.Trim() ?? string.Empty;
        WinnerSupport = winnerSupport;
        LoserSupport = loserSupport;
    }

    /// <summary>
    /// Deconstructs the election into its constituent components.
    /// </summary>
    public void Deconstruct(
        out string countryId,
        out RepublicTime boundary,
        out string winnerPartyName,
        out string loserPartyName,
        out double winnerSupport,
        out double loserSupport)
    {
        countryId = CountryId;
        boundary = Boundary;
        winnerPartyName = WinnerPartyName;
        loserPartyName = LoserPartyName;
        winnerSupport = WinnerSupport;
        loserSupport = LoserSupport;
    }

    /// <summary>
    /// Returns a formatted representation of the election outcome.
    /// </summary>
    public override string ToString() =>
        $"{WinnerPartyName} won at {Boundary} ({WinnerPartyName}: {WinnerSupport:0.##}%, {LoserPartyName}: {LoserSupport:0.##}%)";

    /// <summary>
    /// Implicitly converts a <see cref="CountryElection"/> to a boolean indicating whether the election completed.
    /// </summary>
    public static implicit operator bool(CountryElection? election) => election != null;
}
