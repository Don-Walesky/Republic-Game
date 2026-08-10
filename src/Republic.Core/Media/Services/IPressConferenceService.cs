namespace Republic.Core.Media.Services;

using Republic.Core.Media.Models;

/// <summary>
/// Service interface conducting presidential press conferences and journalist Q&A sessions.
/// </summary>
public interface IPressConferenceService
{
    Task<PressConferenceQuestion> HostPressConferenceAsync(string topic, CancellationToken cancellationToken = default);
    Task<bool> AnswerQuestionAsync(string questionId, string optionId, CancellationToken cancellationToken = default);
}
