namespace Republic.Core.Tests.Bridge;

using Republic.Core.Decisions.Models;
using Republic.Core.Workspace.Models;
using Republic.Unity.Bridge;

public sealed class UnityBridgeAudioTests
{
    [Fact]
    public void PhoneRinging_FiresAudioDelegate()
    {
        var bridge = new RepublicUnityBridge();
        PhoneCall? callTriggered = null;

        bridge.PhoneRinging += call => callTriggered = call;
        bridge.OnPhoneRinging(new PhoneCall { CallerName = "Chief of Staff" });

        Assert.NotNull(callTriggered);
        Assert.Equal("Chief of Staff", callTriggered.CallerName);
    }

    [Fact]
    public void EmailReceived_FiresAudioDelegate()
    {
        var bridge = new RepublicUnityBridge();
        EmailMessage? emailTriggered = null;

        bridge.EmailReceived += email => emailTriggered = email;
        bridge.OnEmailReceived(new EmailMessage { Subject = "Budget Deficit Warning" });

        Assert.NotNull(emailTriggered);
        Assert.Equal("Budget Deficit Warning", emailTriggered.Subject);
    }

    [Fact]
    public void AppointmentReminded_FiresAudioDelegate()
    {
        var bridge = new RepublicUnityBridge();
        CalendarAppointment? appointmentTriggered = null;

        bridge.AppointmentReminded += appt => appointmentTriggered = appt;
        bridge.OnAppointmentReminded(new CalendarAppointment { Title = "Cabinet Briefing" });

        Assert.NotNull(appointmentTriggered);
        Assert.Equal("Cabinet Briefing", appointmentTriggered.Title);
    }
}
