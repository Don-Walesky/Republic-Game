namespace Republic.Unity.Visuals;

using UnityEngine;
using Republic.Core.Military.Models;
using Republic.Unity.Bridge;

/// <summary>
/// Unity visual controller managing office environment lighting (Day, Night, Emergency Red Alert during DEFCON 1) and screen shake.
/// </summary>
public sealed class OfficeLightingManager : MonoBehaviour
{
    [Header("Lighting Assets")]
    [SerializeField] private Light mainDirectionalLight = null!;
    [SerializeField] private Light deskLampLight = null!;

    [Header("Lighting Presets")]
    [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.85f);
    [SerializeField] private Color nightColor = new Color(0.15f, 0.2f, 0.35f);
    [SerializeField] private Color emergencyRedColor = new Color(0.9f, 0.05f, 0.05f);

    [Header("Camera Shake FX")]
    [SerializeField] private Transform cameraTransform = null!;

    private DefconLevel currentDefcon = DefconLevel.Defcon5_Peace;

    private void Start()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.DefconLevelChanged += OnDefconChanged;
            bridge.CrisisTriggered += OnCrisisTriggered;
        }
    }

    public void SetOfficeLighting(Color lightColor, float intensity)
    {
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.color = lightColor;
            mainDirectionalLight.intensity = intensity;
        }
    }

    private void OnDefconChanged(DefconLevel prev, DefconLevel next)
    {
        currentDefcon = next;
        if (next == DefconLevel.Defcon1_MaximumReadiness)
        {
            SetOfficeLighting(emergencyRedColor, 1.8f);
            TriggerCameraShake(0.5f, 0.3f);
        }
        else
        {
            SetOfficeLighting(dayColor, 1.0f);
        }
    }

    private void OnCrisisTriggered(string title, string category, string severity)
    {
        if (severity.Equals("Critical", System.StringComparison.OrdinalIgnoreCase))
        {
            TriggerCameraShake(0.3f, 0.15f);
        }
    }

    public void TriggerCameraShake(float duration, float magnitude)
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            StartCoroutine(PerformCameraShake(duration, magnitude));
        }
    }

    private System.Collections.IEnumerator PerformCameraShake(float duration, float magnitude)
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cameraTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPos;
    }

    private void OnDestroy()
    {
        if (RepublicGameManager.Instance != null)
        {
            var bridge = RepublicGameManager.Instance.UnityBridge;
            bridge.DefconLevelChanged -= OnDefconChanged;
            bridge.CrisisTriggered -= OnCrisisTriggered;
        }
    }
}
