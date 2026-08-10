using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Republic Unity bootstrap initialized.");

        var officeHost = new GameObject("OfficeHost");
        officeHost.transform.SetParent(transform, false);
        officeHost.AddComponent<OfficeController>();
        officeHost.AddComponent<OfficeInterfaceInitializer>();
    }
}
