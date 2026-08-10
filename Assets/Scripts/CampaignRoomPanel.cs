using System;
using UnityEngine;
using UnityEngine.UI;

public class CampaignRoomPanel : BaseRoomPanel
{
    protected override string RoomTitle() => "Campaign Office";

    protected override void BuildContent(Transform root)
    {
        UiHelper.Label(root, "Manage campaign strategy with stakeholders and lobby pressure. Promises shape governance, secret deals can return as scandals.", 16,
            new Vector2(0.04f, 0.76f), new Vector2(0.96f, 0.89f));

        var stats = UiHelper.Label(root, string.Empty, 17,
            new Vector2(0.04f, 0.62f), new Vector2(0.96f, 0.75f));
        RefreshStats(stats);

        var actions = UiHelper.Column(root, "CampaignActions",
            new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.60f), spacing: 10f);

        UiHelper.ActionButton(actions.transform, "Accept Stakeholder Demand", () =>
        {
            try
            {
                if (Office.State.CurrentStakeholderId == Guid.Empty)
                {
                    Feedback("No active stakeholder meeting.");
                    return;
                }

                Office.Bridge.ResolveCampaignMeeting(Office.State.CurrentStakeholderId, "AcceptDemand");
                Office.SyncFromBridge("Accepted stakeholder demand.");
                RefreshStats(stats);
                Feedback("Demand accepted. Polling and endorsements increased.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Counter-Offer Stakeholder", () =>
        {
            try
            {
                if (Office.State.CurrentStakeholderId == Guid.Empty)
                {
                    Feedback("No active stakeholder meeting.");
                    return;
                }

                Office.Bridge.ResolveCampaignMeeting(Office.State.CurrentStakeholderId, "CounterOffer");
                Office.SyncFromBridge("Counter-offered stakeholder demand.");
                RefreshStats(stats);
                Feedback("Counter-offer made. Moderate campaign gains.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Gentlemen's Agreement", () =>
        {
            try
            {
                if (Office.State.CurrentStakeholderId == Guid.Empty)
                {
                    Feedback("No active stakeholder meeting.");
                    return;
                }

                Office.Bridge.ResolveCampaignMeeting(Office.State.CurrentStakeholderId, "GentlemensAgreement");
                Office.SyncFromBridge("Entered a private campaign agreement.");
                RefreshStats(stats);
                Feedback("Backroom deal secured. Scandal risk increased.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Decline Stakeholder Demand", () =>
        {
            try
            {
                if (Office.State.CurrentStakeholderId == Guid.Empty)
                {
                    Feedback("No active stakeholder meeting.");
                    return;
                }

                Office.Bridge.ResolveCampaignMeeting(Office.State.CurrentStakeholderId, "Decline");
                Office.SyncFromBridge("Declined stakeholder demand.");
                RefreshStats(stats);
                Feedback("Demand declined. Polling impact expected.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Expose Scandals Check", () =>
        {
            try
            {
                Office.Bridge.SimulateSecretDealExposure();
                Office.SyncFromBridge("Campaign scandal exposure check executed.");
                RefreshStats(stats);
                Feedback("Exposure simulation complete.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });

        UiHelper.ActionButton(actions.transform, "Election Night", () =>
        {
            try
            {
                var won = Office.Bridge.ResolveElectionNight();
                Office.SyncFromBridge(won ? "Election won." : "Election lost; AI caretaker installed.");
                RefreshStats(stats);
                Feedback(won ? "You won the election." : "You lost. Rebuild campaign momentum.");
            }
            catch (Exception ex)
            {
                Feedback(ex.Message);
            }
        });
    }

    private void RefreshStats(Text label)
    {
        label.text =
            $"Polling: {Office.State.CampaignPolling:0.0}  |  Funding: {Office.State.CampaignFunding:0.0}  |  Endorsements: {Office.State.CampaignEndorsements:0.0}  |  Blocs: {Office.State.CampaignVotingBlocs:0.0}" +
            $"\nStakeholder: {(string.IsNullOrWhiteSpace(Office.State.CurrentStakeholderName) ? "None" : Office.State.CurrentStakeholderName)}" +
            $"  Demand: {(string.IsNullOrWhiteSpace(Office.State.CurrentStakeholderDemand) ? "None" : Office.State.CurrentStakeholderDemand)}";
    }
}
