namespace Republic.Unity.Desk;

using UnityEngine;

/// <summary>
/// Raycasts mouse clicks from the main camera to detect and trigger 3D executive desk interactions.
/// </summary>
public sealed class ExecutiveDeskRaycaster : MonoBehaviour
{
    [SerializeField] private Camera mainCamera = null!;
    [SerializeField] private float maxRaycastDistance = 50f;

    private InteractiveDeskItem currentHoveredItem = null!;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main!;
        }
    }

    private void Update()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance))
        {
            var deskItem = hit.collider.GetComponent<InteractiveDeskItem>();
            if (deskItem != null)
            {
                if (currentHoveredItem != deskItem)
                {
                    currentHoveredItem?.OnHoverExit();
                    currentHoveredItem = deskItem;
                    currentHoveredItem.OnHoverEnter();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    currentHoveredItem.Interact();
                }
                return;
            }
        }

        if (currentHoveredItem != null)
        {
            currentHoveredItem.OnHoverExit();
            currentHoveredItem = null!;
        }
    }
}
