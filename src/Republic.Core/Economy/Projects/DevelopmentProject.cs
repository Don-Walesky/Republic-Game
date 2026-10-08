namespace Republic.Core.Economy.Projects;

using System.Text.Json.Serialization;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Abstract foundation for sovereign capital development projects.
/// Encapsulates project identity, sovereign ownership, economic cost, timing boundaries,
/// state lifecycle (active / completed), deterministic completion, and isolated effect application.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(NationalRoadProject), typeDiscriminator: nameof(NationalRoadProject))]
public abstract class DevelopmentProject : IDevelopmentProject
{
    /// <summary>
    /// Gets or sets the unique identifier of the development project.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the unique identifier of the sovereign country that owns and funded this project.
    /// Every development project must belong to exactly one valid sovereign nation.
    /// Enforces strict country isolation.
    /// </summary>
    public string CountryId { get; set; } = string.Empty;

    /// <summary>
    /// Gets the categorical development project type.
    /// </summary>
    public abstract DevelopmentProjectType ProjectType { get; }

    /// <summary>
    /// Gets the human-readable display name of the development project.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets or sets the liquid REPU cost withdrawn upon initiation.
    /// </summary>
    public double Cost { get; set; }

    /// <summary>
    /// Gets or sets the authoritative Republic simulation timestamp / boundary at which construction started.
    /// </summary>
    public RepublicTime StartedBoundary { get; set; }

    /// <summary>
    /// Gets or sets the authoritative Republic simulation timestamp / boundary at which construction finishes.
    /// </summary>
    public RepublicTime FinishBoundary { get; set; }

    /// <summary>
    /// Alias for <see cref="StartedBoundary"/>.
    /// </summary>
    [JsonIgnore]
    public RepublicTime StartedTime => StartedBoundary;

    /// <summary>
    /// Alias for <see cref="FinishBoundary"/>.
    /// </summary>
    [JsonIgnore]
    public RepublicTime FinishTime => FinishBoundary;

    /// <summary>
    /// Gets or sets a value indicating whether construction of the project has completed.
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether construction of the project has completed.
    /// Synchronized with <see cref="Completed"/>.
    /// </summary>
    [JsonIgnore]
    public bool IsCompleted
    {
        get => Completed;
        set => Completed = value;
    }

    /// <summary>
    /// Gets a value indicating whether the project is currently in progress and active.
    /// </summary>
    [JsonIgnore]
    public bool IsActive => !Completed;

    /// <summary>
    /// Determines whether the project can be completed at the specified simulation time.
    /// Requires a valid non-empty CountryId and that the finish boundary has been reached.
    /// </summary>
    public virtual bool CanComplete(RepublicTime currentTime) =>
        !Completed && !string.IsNullOrWhiteSpace(CountryId) && currentTime >= FinishBoundary;

    /// <summary>
    /// Completes the development project for the owning sovereign country.
    /// Idempotent: applies the completion effect exactly once; subsequent invocations return false.
    /// Enforces strict country isolation: refuses completion if passed a country other than the owner,
    /// or if CountryId is missing or invalid.
    /// </summary>
    public bool Complete(Country country, RepublicTime? currentTime = null)
    {
        ArgumentNullException.ThrowIfNull(country);

        // Project must have a valid non-empty CountryId belonging strictly to this country
        if (string.IsNullOrWhiteSpace(CountryId) ||
            string.IsNullOrWhiteSpace(country.Id) ||
            !string.Equals(CountryId, country.Id, StringComparison.Ordinal))
        {
            return false;
        }

        // Idempotency: cannot apply completion twice
        if (Completed)
        {
            return false;
        }

        // Simulation clock check: must have reached finish boundary if a time is specified
        if (currentTime.HasValue && currentTime.Value < FinishBoundary)
        {
            return false;
        }

        Completed = true;
        ApplyCompletionEffect(country);
        country.RecalculateStateCapacity();
        return true;
    }

    /// <summary>
    /// Applies the project's unique national effect upon completion.
    /// </summary>
    protected abstract void ApplyCompletionEffect(Country country);
}
