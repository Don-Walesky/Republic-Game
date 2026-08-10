using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public sealed class UnityGovernmentBridge
{
    private readonly object _bridge;
    private readonly Type _bridgeType;

    public UnityGovernmentBridge()
    {
        var assemblyPath = Path.Combine(Application.dataPath, "..", "src", "Republic.Core", "bin", "Debug", "net8.0", "Republic.Core.dll");
        var assemblyPathFull = Path.GetFullPath(assemblyPath);
        var assembly = Assembly.LoadFrom(assemblyPathFull);
        var type = assembly.GetType("Republic.Core.Government.GovernmentStateBridge");
        _bridge = Activator.CreateInstance(type!);
        _bridgeType = type!;
    }

    private object Invoke(string method, params object[] args) =>
        _bridgeType.GetMethod(method)!.Invoke(_bridge, args);

    private void InvokeVoid(string method, params object[] args) =>
        _bridgeType.GetMethod(method)!.Invoke(_bridge, args);

    public object CreateInitialState(string countryName, string leaderName, string currencyCode, decimal treasuryBalance, decimal initialExchangeRate)
        => Invoke("CreateInitialState", countryName, leaderName, currencyCode, treasuryBalance, initialExchangeRate);

    public object GetState() => Invoke("GetState");

    public object GetSnapshot() => Invoke("GetSnapshot");

    public void WinElection() => InvokeVoid("WinElection");

    public void AssignCabinetMember(string ministerName, string portfolio)
        => InvokeVoid("AssignCabinetMember", ministerName, portfolio);

    public void ReviewAndAssignCabinetMember(string ministerName, string portfolio, int senateScore)
        => Invoke("ReviewAndAssignCabinetMember", ministerName, portfolio, senateScore);

    public void IssueMinisterialTask(string ministerName, string task, decimal cost)
        => Invoke("IssueMinisterialTask", ministerName, task, cost);

    public object CreateEmploymentProgram(string name, int jobsCreated, decimal cost)
        => Invoke("CreateEmploymentProgram", name, jobsCreated, cost);

    public object ExecuteTradeThroughMinister(string ministerName, string partnerCountry, decimal amount, decimal newExchangeRate)
        => Invoke("ExecuteTradeThroughMinister", ministerName, partnerCountry, amount, newExchangeRate);

    public object SponsorBill(string title, string sponsor, decimal cost)
        => Invoke("SponsorBill", title, sponsor, cost);

    public void NegotiateBill(Guid billId, decimal negotiatedCost, int supportScore)
        => Invoke("NegotiateBill", billId, negotiatedCost, supportScore);

    public void NegotiateForeignAid(string partnerCountry, string aidType, decimal amount, decimal interestRate, decimal returnValue)
        => Invoke("NegotiateForeignAid", partnerCountry, aidType, amount, interestRate, returnValue);

    public void HoldPressBriefing(string message, int mediaBoost)
        => InvokeVoid("HoldPressBriefing", message, mediaBoost);

    public void PrepareForNextElection(decimal campaignBudget)
        => InvokeVoid("PrepareForNextElection", campaignBudget);

    public void RecruitPersonnel(int count, decimal cost)
        => InvokeVoid("RecruitPersonnel", count, cost);

    public void PurchaseWeapons(int weapons, decimal cost)
        => InvokeVoid("PurchaseWeapons", weapons, cost);

    public object AdvanceTurn()
        => Invoke("AdvanceTurn");

    public void AddIndustry(string name, string sector, decimal cost)
        => Invoke("AddIndustry", name, sector, cost);

    public void DiscoverResource(string name, string type)
        => Invoke("DiscoverResource", name, type);

    public void BeginResourceExtraction(string name, decimal cost)
        => InvokeVoid("BeginResourceExtraction", name, cost);

    public void BuildInfrastructure(string type, decimal cost)
        => Invoke("BuildInfrastructure", type, cost);

    public object TakeLoan(string lender, decimal amount, decimal interestRate, int turns)
        => Invoke("TakeLoan", lender, amount, interestRate, turns);

    public void RepayLoan(Guid loanId, decimal amount)
        => InvokeVoid("RepayLoan", loanId, amount);

    public void RenegotiateLoan(Guid loanId, decimal newRate, int extraTurns)
        => InvokeVoid("RenegotiateLoan", loanId, newRate, extraTurns);

    public object LaunchMilitaryOperation(string target, string opType, int troops)
        => Invoke("LaunchMilitaryOperation", target, opType, troops);

    public void ReceiveMilitaryAttack(string attacker, int strength)
        => InvokeVoid("ReceiveMilitaryAttack", attacker, strength);

    public void NegotiateBilateralTrade(string partner, decimal exports, decimal imports)
        => Invoke("NegotiateBilateralTrade", partner, exports, imports);

    public void GrantLegislatorDemand(Guid demandId, bool grant)
        => InvokeVoid("GrantLegislatorDemand", demandId, grant);

    public void AppointJudge(string name, int score)
        => Invoke("AppointJudge", name, score);

    public void RuleOnLaw(Guid billId)
        => Invoke("RuleOnLaw", billId);

    public void RespondToDisaster(string type, decimal budget)
        => Invoke("RespondToDisaster", type, budget);

    public object ReceiveMinisterInitiative(string minister, string title, decimal cost, decimal gdpImpact)
        => Invoke("ReceiveMinisterInitiative", minister, title, cost, gdpImpact);

    public void ApproveMinisterInitiative(Guid id, bool approve)
        => InvokeVoid("ApproveMinisterInitiative", id, approve);

    public void AddressProtest(string concession, decimal cost)
        => InvokeVoid("AddressProtest", concession, cost);

    public void RunIntelligenceOp(string opType, string target, decimal cost)
        => Invoke("RunIntelligenceOp", opType, target, cost);

    public void InvestigateCorruption(decimal cost)
        => InvokeVoid("InvestigateCorruption", cost);

    public void ResolveCampaignMeeting(Guid stakeholderId, string choice)
        => InvokeVoid("ResolveCampaignMeeting", stakeholderId, choice);

    public bool ResolveElectionNight()
        => (bool)Invoke("ResolveElectionNight");

    public void SimulateSecretDealExposure()
        => InvokeVoid("SimulateSecretDealExposure");

    public void UpdatePlayerPresence(int consecutiveOfflineTurns, int toppleThresholdTurns)
        => InvokeVoid("UpdatePlayerPresence", consecutiveOfflineTurns, toppleThresholdTurns);

    public void RecalculateForexMarket()
        => InvokeVoid("RecalculateForexMarket");

    public void FormAlliance(string allianceName, string type, string[] members)
        => Invoke("FormAlliance", allianceName, type, members);

    public void RecordMilitaryIncident(string initiator, string target, string incidentType, string severity)
        => Invoke("RecordMilitaryIncident", initiator, target, incidentType, severity);

    public void RunMidtermElection()
        => InvokeVoid("RunMidtermElection");

    public void ResolveDefectionCrisis(string response)
        => InvokeVoid("ResolveDefectionCrisis", response);
}
