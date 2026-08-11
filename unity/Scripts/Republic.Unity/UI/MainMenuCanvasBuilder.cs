namespace Republic.Unity.UI;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity MonoBehaviour constructing procedural layout components for the Main Menu & Campaign Scenario Selection screen.
/// </summary>
public sealed class MainMenuCanvasBuilder : MonoBehaviour
{
    [SerializeField] private Canvas menuCanvas = null!;
    [SerializeField] private CanvasScaler canvasScaler = null!;
    [SerializeField] private GraphicRaycaster graphicRaycaster = null!;

    public void BuildMainMenuLayout()
    {
        if (menuCanvas == null)
        {
            menuCanvas = gameObject.GetComponent<Canvas>();
            if (menuCanvas == null)
            {
                menuCanvas = gameObject.AddComponent<Canvas>();
            }
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
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

        if (gameObject.GetComponent<MainMenuUIController>() == null)
        {
            gameObject.AddComponent<MainMenuUIController>();
        }

        Debug.Log("[Republic UI] Main Menu & Campaign Selector canvas layout built cleanly.");
    }
}
