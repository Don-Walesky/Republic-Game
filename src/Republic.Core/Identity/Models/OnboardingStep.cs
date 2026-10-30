namespace Republic.Core.Identity.Models;

/// <summary>
/// Discrete stages of the new player onboarding journey in Republic.
/// Used to track player progression and cleanly resume incomplete onboarding.
/// </summary>
public enum OnboardingStep
{
    /// <summary>
    /// Initial uninitialized state. No registration has occurred.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Player has registered account details (Name, Email, Date of Birth). Awaiting email verification.
    /// </summary>
    AccountRegistered = 1,

    /// <summary>
    /// Player has successfully verified their email address. Eligible to create a sovereign country.
    /// </summary>
    EmailVerified = 2,

    /// <summary>
    /// Player has founded their sovereign country through procedural world generation.
    /// </summary>
    CountryCreated = 3,

    /// <summary>
    /// Player has chosen exactly two leadership types for their leadership profile.
    /// </summary>
    LeadershipSelected = 4,

    /// <summary>
    /// Player has chosen exactly four national development priorities for their national agenda.
    /// </summary>
    DevelopmentPlansSelected = 5,

    /// <summary>
    /// Player has been introduced to the General Adviser, Press Secretary, and Political/Social Affairs Special Adviser.
    /// </summary>
    AdvisersIntroduced = 6,

    /// <summary>
    /// Player has entered Campaign Headquarters to manage voting blocs, promises, and contest the election.
    /// </summary>
    CampaignHeadquarters = 7,

    /// <summary>
    /// Player has won the election and assumed office in the Presidential Executive Office.
    /// </summary>
    InOffice = 8
}

