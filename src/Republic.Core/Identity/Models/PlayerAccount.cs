namespace Republic.Core.Identity.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Domain model representing a registered player account and onboarding progression.
/// Preserves personal data privacy by isolating sensitive identity from public game states.
/// </summary>
public sealed class PlayerAccount
{
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("current_step")]
    public OnboardingStep CurrentStep { get; set; } = OnboardingStep.NotStarted;

    [JsonPropertyName("personal_identity")]
    public PlayerPersonalIdentity PersonalIdentity { get; set; } = new();

    [JsonPropertyName("owned_country_id")]
    public string? OwnedCountryId { get; set; }

    [JsonPropertyName("selected_leadership_archetypes")]
    public List<LeadershipArchetype> SelectedLeadershipArchetypes { get; set; } = new();

    [JsonPropertyName("leadership_doctrine_title")]
    public string LeadershipDoctrineTitle { get; set; } = string.Empty;

    [JsonPropertyName("selected_development_plans")]
    public List<NationalDevelopmentPriority> SelectedDevelopmentPlans { get; set; } = new();

    [JsonPropertyName("last_login_utc")]
    public DateTimeOffset LastLoginUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a sanitized, public-facing representation of the player's identity suitable for diplomatic profiles.
    /// Never exposes email address, real name, or date of birth.
    /// </summary>
    public string GetPublicIdentity()
    {
        if (!string.IsNullOrWhiteSpace(DisplayName))
        {
            return DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(PersonalIdentity.LastName))
        {
            return $"Leader {PersonalIdentity.LastName}";
        }

        return "Sovereign Executive";
    }
}

