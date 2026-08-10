namespace Republic.Core.Workspace.Services;

using Republic.Core.Workspace.Models;

/// <summary>
/// Service interface for handling phone calls in the executive workspace.
/// </summary>
public interface IPhoneService
{
    void ReceiveCall(PhoneCall call);
    PhoneCall? GetActiveCall();
    bool AnswerCall(string callId);
    bool RejectCall(string callId);
    IReadOnlyList<PhoneCall> GetCallHistory();
}
