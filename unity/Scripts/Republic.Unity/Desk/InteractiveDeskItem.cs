namespace Republic.Unity.Desk;

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Unity MonoBehaviour component representing a clickable 3D desk item (e.g. Hotline Phone, Dossier, Stamp).
/// </summary>
public sealed class InteractiveDeskItem : MonoBehaviour
{
    [SerializeField] private DeskItemType itemType = DeskItemType.HotlinePhone;
    [SerializeField] private string itemName = "Hotline Phone";
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private UnityEvent onClicked = new();

    private Renderer itemRenderer = null!;
    private Color originalColor;

    public DeskItemType ItemType => itemType;
    public string ItemName => itemName;

    private void Awake()
    {
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null && itemRenderer.material != null)
        {
            originalColor = itemRenderer.material.color;
        }
    }

    public void OnHoverEnter()
    {
        if (itemRenderer != null && itemRenderer.material != null)
        {
            itemRenderer.material.color = highlightColor;
        }
    }

    public void OnHoverExit()
    {
        if (itemRenderer != null && itemRenderer.material != null)
        {
            itemRenderer.material.color = originalColor;
        }
    }

    public void Interact()
    {
        Debug.Log($"[Executive Desk] Interacted with desk item: '{itemName}' ({itemType})");
        onClicked.Invoke();
    }
}
