namespace Republic.Core.Media.Models;

/// <summary>
/// Categorical editorial leaning of a media organisation in Republic.
/// Governs report framing, bias, credibility, and receptiveness to government influence.
/// </summary>
public enum MediaEditorialLeaning
{
    /// <summary>
    /// Supportive of the executive branch and state institutions. Slow to report scandals.
    /// </summary>
    StateAligned = 0,

    /// <summary>
    /// Independent, investigative, and evidence-driven. Highly resistant to covert influence.
    /// </summary>
    InvestigativeIndependent = 1,

    /// <summary>
    /// Focused on fiscal conditions, market confidence, private investment, and GDP growth.
    /// </summary>
    FinancialBusiness = 2,

    /// <summary>
    /// Sensationalist and populist. Quick to highlight cost of living, corruption, and unrest.
    /// </summary>
    PopulistOpposition = 3
}
