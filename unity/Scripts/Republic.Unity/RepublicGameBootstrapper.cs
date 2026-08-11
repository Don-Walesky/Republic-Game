namespace Republic.Unity;

using UnityEngine;

/// <summary>
/// Scene entry point component instantiating RepublicGameManager and initial canvas UI layout builders.
/// </summary>
public sealed class RepublicGameBootstrapper : MonoBehaviour
{
    private void Start()
    {
        if (RepublicGameManager.Instance == null)
        {
            var managerGo = new GameObject("[RepublicGameManager]");
            managerGo.AddComponent<RepublicGameManager>();
        }

        Debug.Log("[Republic Bootstrapper] Presidential Executive Suite Scene loaded and bootstrapped successfully.");
    }
}
