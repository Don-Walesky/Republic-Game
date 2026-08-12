namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;
using Republic.Core.Military.Models;
using Republic.Unity.Bridge;

/// <summary>
/// Unity UI controller managing defense readiness displays, DEFCON status badges, and strategic operation triggers.
/// </summary>
public sealed class MilitaryDefenseUIController : MonoBehaviour
{
    [Header("Defense Overview Displays")]
    [SerializeField] private Text defconBadgeText = null!;
    [SerializeField] private Text personnelCountText = null!;
    [SerializeField] private Text equipmentCountText = null!;
    [SerializeField] private Text compositeReadinessText = null!;
    [SerializeField] private Text logisticsEfficiencyText = null!;
    [SerializeField] private Text unitTrainingIndexText = null!;

    [Header("Branch Breakdown Texts")]
    [SerializeField] private Text armyReadinessText = null!;
    [SerializeField] private Text navyReadinessText = null!;
    [SerializeField] private Text airForceReadinessText = null!;
    [SerializeField] private Text cyberCorpsReadinessText = null!;

    [Header("Directive Status Log")]
    [SerializeField] private Text directiveOutcomeText = null!;

    private void Start()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.MilitaryReadinessReportUpdated += UpdateReadinessUI;
            bridge.DefconLevelChanged += OnDefconChanged;
            bridge.MilitaryDirectiveExecuted += OnDirectiveExecuted;
        }
    }

    public void SetDefconAlertLevel(int level)
    {
        if (System.Enum.IsDefined(typeof(DefconLevel), level))
        {
            var newLevel = (DefconLevel)level;
            Debug.Log($"[Military Defense UI] Triggered manual DEFCON alert change to: {newLevel}");
        }
    }

    public void UpdateReadinessUI(MilitaryReadinessReport report)
    {
        if (report == null) return;

        if (defconBadgeText != null)
        {
            defconBadgeText.text = $"ALERT: {report.Defcon}";
        }

        if (personnelCountText != null)
        {
            personnelCountText.text = $"Armed Personnel: {report.TotalPersonnel:N0}";
        }

        if (equipmentCountText != null)
        {
            equipmentCountText.text = $"Ordnance & Equipment: {report.TotalEquipment:N0}";
        }

        if (compositeReadinessText != null)
        {
            compositeReadinessText.text = $"Readiness Score: {report.CompositeReadinessScore:0.0}%";
        }

        if (logisticsEfficiencyText != null)
        {
            logisticsEfficiencyText.text = $"Logistics Supply Efficiency: {report.LogisticsSupplyEfficiency:0.0}%";
        }

        if (unitTrainingIndexText != null)
        {
            unitTrainingIndexText.text = $"Unit Training Index: {report.UnitTrainingIndex:0.0}%";
        }

        foreach (var branch in report.BranchBreakdown)
        {
            switch (branch.Branch)
            {
                case MilitaryBranch.Army:
                    if (armyReadinessText != null) armyReadinessText.text = $"Army: {branch.ReadinessScore:0.0}% ({branch.PersonnelCount:N0} troops)";
                    break;
                case MilitaryBranch.Navy:
                    if (navyReadinessText != null) navyReadinessText.text = $"Navy: {branch.ReadinessScore:0.0}% ({branch.EquipmentCount} vessels)";
                    break;
                case MilitaryBranch.AirForce:
                    if (airForceReadinessText != null) airForceReadinessText.text = $"Air Force: {branch.ReadinessScore:0.0}% ({branch.EquipmentCount} aircraft)";
                    break;
                case MilitaryBranch.CyberCorps:
                    if (cyberCorpsReadinessText != null) cyberCorpsReadinessText.text = $"Cyber Corps: {branch.ReadinessScore:0.0}% ({branch.PersonnelCount} operatives)";
                    break;
            }
        }
    }

    public void OnDefconChanged(DefconLevel previousLevel, DefconLevel newLevel)
    {
        if (defconBadgeText != null)
        {
            defconBadgeText.text = $"ALERT CHANGED: {newLevel}";
        }
    }

    public void OnDirectiveExecuted(MilitaryDirectiveResult result)
    {
        if (directiveOutcomeText != null && result != null)
        {
            string outcome = result.Success ? "SUCCESSFUL" : "FAILED";
            directiveOutcomeText.text = $"[DIRECTIVE {outcome}] {result.OperationType} against {result.TargetCountry} - Sustained Casualties: {result.CasualtiesSustained}";
        }
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.MilitaryReadinessReportUpdated -= UpdateReadinessUI;
            bridge.DefconLevelChanged -= OnDefconChanged;
            bridge.MilitaryDirectiveExecuted -= OnDirectiveExecuted;
        }
    }
}
