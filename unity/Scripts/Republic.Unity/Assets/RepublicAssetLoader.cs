namespace Republic.Unity.Assets;

using UnityEngine;

/// <summary>
/// Unity AssetLoader fetching 3D desk models, audio assets, and scenario configuration files dynamically.
/// </summary>
public sealed class RepublicAssetLoader : MonoBehaviour
{
    private static RepublicAssetLoader instance = null!;
    public static RepublicAssetLoader Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Object LoadAssetFromResources(string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath)) return null!;

        Object asset = Resources.Load(resourcePath);
        if (asset == null)
        {
            Debug.LogWarning($"[Republic AssetLoader] Failed to load asset at path: '{resourcePath}'");
        }
        return asset!;
    }

    public T LoadAssetFromResources<T>(string resourcePath) where T : Object
    {
        if (string.IsNullOrWhiteSpace(resourcePath)) return null!;

        T asset = Resources.Load<T>(resourcePath);
        if (asset == null)
        {
            Debug.LogWarning($"[Republic AssetLoader] Failed to load asset of type {typeof(T).Name} at path: '{resourcePath}'");
        }
        return asset!;
    }
}
