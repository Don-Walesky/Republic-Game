using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OfficeStateModel
{
    public string Phase = "Campaign";
    public decimal TreasuryBalance = 500000m;
    public decimal ExchangeRate = 1.0m;
    public int Personnel;
    public int Weapons;
    public int TradeVolume;
    public int EmploymentPrograms;
    public decimal ApprovalRating = 28m;
    public decimal EconomyIndex = 28m;
    public decimal StabilityIndex = 26m;
    public decimal DiplomacyIndex = 24m;
    public int Population = 1000000;
    public decimal Gdp = 20000000m;
    public decimal InfrastructureIndex = 22m;
    public decimal Hdi = 0.31m;
    public decimal EducationIndex = 25m;
    public decimal InsecurityIndex = 55m;
    public decimal UnemploymentRate = 28m;
    public decimal ExternalDebt;
    public int ElectionCycle = 1;
    public int CurrentTurn;
    public int TertiaryInstitutions = 2;
    public decimal CorruptionLevel = 45m;
    public decimal CivilUnrestIndex = 40m;
    public decimal LegislatorLoyalty = 42m;
    public decimal OppositionStrength = 55m;
    public string OppositionLastAction = string.Empty;
    public int SenateSeatsHeld = 60;
    public int HouseSeatsHeld = 190;
    public decimal CampaignPolling = 30m;
    public decimal CampaignFunding = 20m;
    public decimal CampaignEndorsements = 10m;
    public decimal CampaignVotingBlocs = 20m;
    public Guid CurrentStakeholderId;
    public string CurrentStakeholderName = string.Empty;
    public string CurrentStakeholderDemand = string.Empty;
    public bool IsGovernmentToppled;
    public string CaretakerPresident = string.Empty;
    public int NeighborCountries;
    public int ActiveTreaties;
    public string StrongestNeighbor = string.Empty;
    public string StrongestNeighborCurrency = string.Empty;
    public decimal StrongestNeighborPower;
    public int IndustryCount;
    public decimal IndustryTurnIncome;
    public int ResourceCount;
    public int ActiveLoansCount;
    public Guid ActiveLoanId;
    public decimal ActiveLoanOwed;
    public string ActiveLoanLender = string.Empty;
    public int BilateralTradesCount;
    public int BillCount;
    public int ForeignAidCount;
    public int MinisterialTaskCount;
    public int CabinetCount;
    public Guid PendingDemandId;
    public string PendingDemandDesc = string.Empty;
    public decimal PendingDemandCost;
    public Guid PendingInitiativeId;
    public string PendingInitiativeTitle = string.Empty;
    public decimal PendingInitiativeCost;
    public Guid LastBillId;
    public string LastBillTitle = string.Empty;
    public string LastBillStatus = string.Empty;
    public string LastRandomEvent = string.Empty;
    public List<string> ActivityLog = new();
}

public static class OfficeStateStore
{
    private const string Key = "Republic.OfficeState";

    public static OfficeStateModel Load()
    {
        if (!PlayerPrefs.HasKey(Key))
        {
            return new OfficeStateModel();
        }

        var json = PlayerPrefs.GetString(Key);
        var state = JsonUtility.FromJson<OfficeStateModel>(json);
        return state ?? new OfficeStateModel();
    }

    public static void Save(OfficeStateModel state)
    {
        if (state == null)
        {
            return;
        }

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(state));
        PlayerPrefs.Save();
    }
}
