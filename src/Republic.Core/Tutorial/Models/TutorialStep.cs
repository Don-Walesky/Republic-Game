namespace Republic.Core.Tutorial.Models;

/// <summary>
/// Domain model describing a single step in the presidential onboarding tutorial.
/// </summary>
public sealed class TutorialStep
{
    public int StepIndex { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string TargetElementId { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
}
