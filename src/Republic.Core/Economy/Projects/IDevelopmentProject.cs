namespace Republic.Core.Economy.Projects;

using System.Text.Json.Serialization;
using Republic.Core.Time;
using Republic.Core.World.Models;

/// <summary>
/// Contract representing a sovereign capital development project.
/// Encapsulates identity, sovereign country ownership, project type, cost, simulation timing boundaries,
/// completion lifecycle, deterministic progression, and isolated effect application.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(NationalRoadProject), typeDiscriminator: nameof(NationalRoadProject))]
public interface IDevelopmentProject
{
    /// <summary>
    /// Gets the unique identifier of the development project.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the unique identifier of the owning sovereign nation.
    /// Enforces country isolation.
    /// </summary>
    string CountryId { get; }

    /// <summary>
    /// Gets the categorical development project type.
    /// </summary>
    DevelopmentProjectType ProjectType { get; }

    /// <summary>
    /// Gets the display name of the development project.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the liquid REPU cost withdrawn at initiation.
    /// </summary>
    double Cost { get; }

    /// <summary>
    /// Gets the simulation timestamp / boundary at which construction started.
    /// </summary>
    RepublicTime StartedBoundary { get; }

    /// <summary>
    /// Gets the simulation timestamp / boundary at which construction finishes.
    /// </summary>
    RepublicTime FinishBoundary { get; }

    /// <summary>
    /// Gets the simulation timestamp at which construction started.
    /// </summary>
    RepublicTime StartedTime { get; }

    /// <summary>
    /// Gets the simulation timestamp at which construction finishes.
    /// </summary>
    RepublicTime FinishTime { get; }

    /// <summary>
    /// Gets or sets a value indicating whether construction of the project has completed.
    /// </summary>
    bool Completed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether construction of the project has completed.
    /// </summary>
    bool IsCompleted { get; set; }

    /// <summary>
    /// Gets a value indicating whether the project is currently in progress.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Evaluates whether the project can be completed at the specified simulation time.
    /// </summary>
    bool CanComplete(RepublicTime currentTime);

    /// <summary>
    /// Completes the project for the specified sovereign country.
    /// Idempotent: applies effects exactly once; subsequent calls return false.
    /// Enforces country isolation: returns false if the passed country does not match CountryId.
    /// </summary>
    bool Complete(Country country, RepublicTime? currentTime = null);
}
