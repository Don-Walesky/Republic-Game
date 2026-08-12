namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity MonoBehaviour building procedural presidential office desk UI canvas layouts and binding UI controllers.
/// </summary>
public sealed class ExecutiveDeskCanvasBuilder : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas = null!;
    [SerializeField] private CanvasScaler canvasScaler = null!;
    [SerializeField] private GraphicRaycaster graphicRaycaster = null!;

    [Header("Procedural Action Panels")]
    [SerializeField] private GameObject actionButtonsPanel = null!;
    [SerializeField] private GameObject visitorLobbyPanel = null!;
    [SerializeField] private GameObject calendarAlertPanel = null!;

    public void BuildDeskCanvasLayout()
    {
        if (targetCanvas == null)
        {
            targetCanvas = gameObject.GetComponent<Canvas>();
            if (targetCanvas == null)
            {
                targetCanvas = gameObject.AddComponent<Canvas>();
            }
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (canvasScaler == null)
        {
            canvasScaler = gameObject.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
            }
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
        }

        if (graphicRaycaster == null)
        {
            graphicRaycaster = gameObject.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Ensure Core UI Controllers are attached
        EnsureController<ExecutiveDeskUIController>();
        EnsureController<MilitaryDefenseUIController>();
        EnsureController<DecisionPromptUIController>();
        EnsureController<IntelligenceMediaUIController>();
        EnsureController<WorkspaceUIController>();

        Debug.Log("[Republic UI] Executive Desk Canvas procedural layout & UI controllers constructed cleanly.");
    }

    private void EnsureController<T>() where T : MonoBehaviour
    {
        if (gameObject.GetComponent<T>() == null)
        {
            gameObject.AddComponent<T>();
        }
    }
}

