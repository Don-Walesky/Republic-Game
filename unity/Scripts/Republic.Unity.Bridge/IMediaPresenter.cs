namespace Republic.Unity.Bridge;

using System.Collections.Generic;

/// <summary>
/// Bridge interface exposing press conference announcements, media headlines, and public approval meters to Unity views.
/// </summary>
public interface IMediaPresenter
{
    void OnPressConferenceConducted(string topic, double publicApprovalDelta, string transcriptSummary);
    void OnHeadlinePublished(string outletName, string headlineText, string category);
    void OnPublicApprovalRatingUpdated(double approvalRating);
}
