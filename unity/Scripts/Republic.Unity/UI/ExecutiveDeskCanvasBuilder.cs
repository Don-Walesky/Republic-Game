namespace Republic.Unity.UI;

using UnityEngine;

/// <summary>
/// Unity MonoBehaviour building procedural presidential office desk UI canvas layouts.
/// </summary>
public sealed class ExecutiveDeskCanvasBuilder : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas = null!;

    public void BuildDeskCanvasLayout()
    {
        if (targetCanvas == null)
        {
            targetCanvas = gameObject.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        Debug.Log("[Republic UI] Executive Desk Canvas procedural layout constructed cleanly.");
    }
}
