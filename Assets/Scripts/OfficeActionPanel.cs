using UnityEngine;
using UnityEngine.UI;

public class OfficeActionPanel : MonoBehaviour
{
    [SerializeField] private OfficeController officeController;

    private void Start()
    {
        if (officeController == null)
        {
            officeController = FindObjectOfType<OfficeController>();
        }
    }

    public void TriggerElection()
    {
        officeController?.WinElection();
    }

    public void TriggerEmploymentProgram()
    {
        officeController?.LaunchEmploymentProgram();
    }

    public void TriggerTrade()
    {
        officeController?.ApproveTrade();
    }

    public void TriggerRecruitment()
    {
        officeController?.RecruitPersonnel();
    }

    public void TriggerWeaponsPurchase()
    {
        officeController?.PurchaseWeapons();
    }
}
