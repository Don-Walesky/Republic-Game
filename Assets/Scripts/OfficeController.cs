using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OfficeController : MonoBehaviour
{
    public OfficeStateModel State { get; private set; }
    public UnityGovernmentBridge Bridge { get; private set; }

    private Text _statusBar;
    private GameObject _hubPanel;
    private readonly List<GameObject> _rooms = new();

    private void Start()
    {
        Bridge = new UnityGovernmentBridge();
        State = OfficeStateStore.Load();

        Bridge.CreateInitialState("Arcadia", "Arcadian Crown", "ARC", State.TreasuryBalance, State.ExchangeRate);
        Bridge.AssignCabinetMember("Mina Vale", "Treasury");
        Bridge.AssignCabinetMember("Jonas Reed", "Defense");
        Bridge.AssignCabinetMember("Ariana Vale", "Trade");
        Bridge.AssignCabinetMember("Rina Sol", "Foreign Affairs");
        Bridge.AssignCabinetMember("Kara Dunn", "Chief of Staff");

        SyncFromBridge("Office shell initialized.", advanceTurn: false);
        BuildHub();
        ShowHub();
    }

    private void BuildHub()
    {
        var canvasGO = UiHelper.Canvas(transform, "OfficeMasterCanvas", sortOrder: 0);

        var statusBarPanel = UiHelper.Panel(canvasGO.transform, "StatusBar",
            new Vector2(0f, 0.92f), new Vector2(1f, 1f),
            new Color(0.05f, 0.08f, 0.12f, 0.97f));
        _statusBar = UiHelper.Label(statusBarPanel.transform, string.Empty, 16,
            new Vector2(0.01f, 0f), new Vector2(0.99f, 1f), TextAnchor.MiddleLeft);

        _hubPanel = UiHelper.Panel(canvasGO.transform, "HubPanel",
            new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.9f),
            new Color(0.08f, 0.11f, 0.15f, 1f));

        UiHelper.Label(_hubPanel.transform, "Executive Office", 30,
            new Vector2(0f, 0.85f), new Vector2(1f, 0.97f), TextAnchor.MiddleCenter).color = new Color(0.9f, 0.85f, 0.6f);

        var navHolder = UiHelper.Row(_hubPanel.transform, "Nav",
            new Vector2(0.03f, 0.6f), new Vector2(0.97f, 0.82f), spacing: 10f);

        UiHelper.NavButton(navHolder.transform, "Campaign", () => OpenRoom<CampaignRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "Military", () => OpenRoom<MilitaryRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "Ministries", () => OpenRoom<MinistriesRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "Legislature", () => OpenRoom<LegislatureRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "Diplomacy", () => OpenRoom<DiplomacyRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "Press", () => OpenRoom<PressRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "Economy", () => OpenRoom<EconomyRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "Intel", () => OpenRoom<IntelRoomPanel>());
        UiHelper.NavButton(navHolder.transform, "World", () => OpenRoom<WorldRoomPanel>());

        var logHolder = UiHelper.Panel(_hubPanel.transform, "LogHolder",
            new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.58f),
            new Color(0.06f, 0.09f, 0.12f, 0.9f));
        var logLabel = UiHelper.Label(logHolder.transform, string.Empty, 16,
            new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), TextAnchor.UpperLeft);
        logLabel.name = "HubLog";
    }

    private void OpenRoom<T>() where T : BaseRoomPanel
    {
        CloseAllRooms();
        var room = gameObject.AddComponent<T>();
        room.Initialize(this);
        _rooms.Add(room.Panel);
        _hubPanel.SetActive(false);
    }

    public void CloseAllRooms()
    {
        foreach (var room in _rooms)
        {
            Destroy(room);
        }

        _rooms.Clear();
        _hubPanel.SetActive(true);
        RefreshStatusBar();
        UpdateHubLog();
    }

    private void ShowHub()
    {
        RefreshStatusBar();
        UpdateHubLog();
    }

    public void SyncFromBridge(string entry, bool advanceTurn = true)
    {
        var didAdvanceTurn = false;
        if (advanceTurn && !string.IsNullOrWhiteSpace(entry))
        {
            Bridge.AdvanceTurn();
            didAdvanceTurn = true;
        }

        var snapshot = (dynamic)Bridge.GetSnapshot();
        State.Phase = snapshot.Phase;
        State.TreasuryBalance = snapshot.TreasuryBalance;
        State.ExchangeRate = snapshot.ExchangeRate;
        State.TradeVolume = (int)snapshot.TradeVolume;
        State.EmploymentPrograms = snapshot.EmploymentPrograms;
        State.Personnel = snapshot.Personnel;
        State.Weapons = snapshot.Weapons;
        State.CabinetCount = snapshot.CabinetCount;

        State.ApprovalRating = snapshot.ApprovalRating;
        State.EconomyIndex = snapshot.EconomyIndex;
        State.StabilityIndex = snapshot.StabilityIndex;
        State.DiplomacyIndex = snapshot.DiplomacyIndex;

        State.Population = snapshot.Population;
        State.Gdp = snapshot.Gdp;
        State.InfrastructureIndex = snapshot.InfrastructureIndex;
        State.Hdi = snapshot.Hdi;
        State.EducationIndex = snapshot.EducationIndex;
        State.InsecurityIndex = snapshot.InsecurityIndex;
        State.UnemploymentRate = snapshot.UnemploymentRate;
        State.ExternalDebt = snapshot.ExternalDebt;

        State.ElectionCycle = snapshot.ElectionCycle;
        State.CurrentTurn = snapshot.CurrentTurn;
        State.TertiaryInstitutions = snapshot.TertiaryInstitutions;
        State.CorruptionLevel = snapshot.CorruptionLevel;
        State.CivilUnrestIndex = snapshot.CivilUnrestIndex;
        State.LegislatorLoyalty = snapshot.LegislatorLoyalty;
        State.OppositionStrength = snapshot.OppositionStrength;
        State.OppositionLastAction = snapshot.OppositionLastAction;

        State.SenateSeatsHeld = snapshot.SenateSeatsHeld;
        State.HouseSeatsHeld = snapshot.HouseSeatsHeld;
        State.CampaignPolling = snapshot.CampaignPolling;
        State.CampaignFunding = snapshot.CampaignFunding;
        State.CampaignEndorsements = snapshot.CampaignEndorsements;
        State.CampaignVotingBlocs = snapshot.CampaignVotingBlocs;
        State.CurrentStakeholderId = snapshot.CurrentStakeholderId;
        State.CurrentStakeholderName = snapshot.CurrentStakeholderName;
        State.CurrentStakeholderDemand = snapshot.CurrentStakeholderDemand;

        State.IsGovernmentToppled = snapshot.IsGovernmentToppled;
        State.CaretakerPresident = snapshot.CaretakerPresident;
        State.NeighborCountries = snapshot.NeighborCountries;
        State.ActiveTreaties = snapshot.ActiveTreaties;
        State.StrongestNeighbor = snapshot.StrongestNeighbor;
        State.StrongestNeighborCurrency = snapshot.StrongestNeighborCurrency;
        State.StrongestNeighborPower = snapshot.StrongestNeighborPower;

        State.IndustryCount = snapshot.IndustryCount;
        State.IndustryTurnIncome = snapshot.IndustryTurnIncome;
        State.ResourceCount = snapshot.ResourceCount;
        State.ActiveLoansCount = snapshot.ActiveLoansCount;
        State.ActiveLoanId = snapshot.ActiveLoanId;
        State.ActiveLoanOwed = snapshot.ActiveLoanOwed;
        State.ActiveLoanLender = snapshot.ActiveLoanLender;
        State.BilateralTradesCount = snapshot.BilateralTradesCount;
        State.BillCount = snapshot.BillCount;
        State.ForeignAidCount = snapshot.ForeignAidCount;
        State.MinisterialTaskCount = snapshot.MinisterialTaskCount;

        State.PendingDemandId = snapshot.PendingDemandId;
        State.PendingDemandDesc = snapshot.PendingDemandDesc;
        State.PendingDemandCost = snapshot.PendingDemandCost;
        State.PendingInitiativeId = snapshot.PendingInitiativeId;
        State.PendingInitiativeTitle = snapshot.PendingInitiativeTitle;
        State.PendingInitiativeCost = snapshot.PendingInitiativeCost;

        State.LastBillId = snapshot.LastBillId;
        State.LastBillTitle = snapshot.LastBillTitle;
        State.LastBillStatus = snapshot.LastBillStatus;
        State.LastRandomEvent = snapshot.LastRandomEvent;

        if (!string.IsNullOrWhiteSpace(entry))
        {
            State.ActivityLog.Add(didAdvanceTurn ? $"[Turn {State.CurrentTurn}] {entry}" : entry);
        }

        if (didAdvanceTurn)
        {
            if (!string.IsNullOrEmpty(State.LastRandomEvent))
            {
                State.ActivityLog.Add($"Random event: {State.LastRandomEvent}");
            }

            if (!string.IsNullOrEmpty(State.PendingDemandDesc))
            {
                State.ActivityLog.Add($"Legislator request: {State.PendingDemandDesc} (${State.PendingDemandCost:N0})");
            }

            if (!string.IsNullOrEmpty(State.PendingInitiativeTitle))
            {
                State.ActivityLog.Add($"Minister initiative awaiting approval: {State.PendingInitiativeTitle}");
            }
        }

        OfficeStateStore.Save(State);
        RefreshStatusBar();
        UpdateHubLog();
    }

    public void Log(string entry)
    {
        SyncFromBridge(entry, advanceTurn: false);
    }

    private void RefreshStatusBar()
    {
        if (_statusBar == null)
        {
            return;
        }

        _statusBar.text =
            $"Turn: {State.CurrentTurn}/30  |  Phase: {State.Phase}  |  Treasury: ${State.TreasuryBalance:N0}" +
            $"  |  Approval: {State.ApprovalRating:0.0}%  |  Economy: {State.EconomyIndex:0.0}" +
            $"  |  Stability: {State.StabilityIndex:0.0}  |  Diplomacy: {State.DiplomacyIndex:0.0}" +
            $"  |  Corruption: {State.CorruptionLevel:0.0}  |  Unrest: {State.CivilUnrestIndex:0.0}";
    }

    private void UpdateHubLog()
    {
        if (_hubPanel == null)
        {
            return;
        }

        var logLabel = _hubPanel.transform.Find("LogHolder/HubLog")?.GetComponent<Text>();
        if (logLabel == null)
        {
            return;
        }

        var recent = State.ActivityLog.Count > 0
            ? string.Join("\n", State.ActivityLog.GetRange(Math.Max(0, State.ActivityLog.Count - 8), Math.Min(8, State.ActivityLog.Count)))
            : "No recent activity.";

        logLabel.text =
            $"Activity log:\n{recent}\n\n" +
            $"World: neighbors={State.NeighborCountries}, treaties={State.ActiveTreaties}, strongest={State.StrongestNeighbor} ({State.StrongestNeighborCurrency})\n" +
            $"Legislature: Senate={State.SenateSeatsHeld}, House={State.HouseSeatsHeld}, Loyalty={State.LegislatorLoyalty:0.0}%\n" +
            $"Campaign: Poll={State.CampaignPolling:0.0}, Funding={State.CampaignFunding:0.0}, Endorsements={State.CampaignEndorsements:0.0}";
    }
}
