namespace Republic.Unity;

using UnityEngine;
using Republic.App;
using Republic.Unity.Bridge;

/// <summary>
/// Unity Singleton Manager exposing the underlying C# RepublicApplication instance and UnityBridge.
/// </summary>
public sealed class RepublicGameManager : MonoBehaviour
{
    private static RepublicGameManager instance = null!;
    public static RepublicGameManager Instance => instance;

    [SerializeField] private string startingScenarioId = "arcadia-day1";

    public RepublicApplication Application { get; private set; } = null!;
    public RepublicUnityBridge UnityBridge { get; private set; } = null!;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeCoreApplication();
    }

    private void InitializeCoreApplication()
    {
        var bootstrapper = new ApplicationBootstrapper();
        Application = bootstrapper.Bootstrap();
        UnityBridge = new RepublicUnityBridge();

        Debug.Log($"[Republic Core] Application & UnityBridge initialized cleanly. Starting scenario: '{startingScenarioId}'");
    }
}
