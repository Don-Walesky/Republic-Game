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

        // Attach UI controllers if not already present
        if (gameObject.GetComponent<ExecutiveDeskUIController>() == null)
        {
            gameObject.AddComponent<ExecutiveDeskUIController>();
        }

        if (gameObject.GetComponent<MilitaryDefenseUIController>() == null)
        {
            gameObject.AddComponent<MilitaryDefenseUIController>();
        }

        if (gameObject.GetComponent<DecisionPromptUIController>() == null)
        {
            gameObject.AddComponent<DecisionPromptUIController>();
        }

        Debug.Log("[Republic UI] Executive Desk Canvas procedural layout & UI controllers constructed cleanly.");
    }
}
