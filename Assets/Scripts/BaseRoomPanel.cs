using UnityEngine;
using UnityEngine.UI;

public abstract class BaseRoomPanel : MonoBehaviour
{
    protected OfficeController Office;
    public GameObject Panel { get; private set; }
    protected Text FeedbackText;

    public void Initialize(OfficeController office)
    {
        Office = office;
        Panel = BuildRoomCanvas();
        BuildContent(Panel.transform);
    }

    private GameObject BuildRoomCanvas()
    {
        var canvasGO = UiHelper.Canvas(Office.transform, $"{GetType().Name}Canvas", sortOrder: 10);

        var bg = UiHelper.Panel(canvasGO.transform, "BG",
            new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.96f),
            new Color(0.07f, 0.1f, 0.14f, 0.98f));

        UiHelper.Label(bg.transform, RoomTitle(), 26,
            new Vector2(0f, 0.9f), new Vector2(1f, 0.99f), TextAnchor.MiddleCenter).color = new Color(0.9f, 0.85f, 0.6f);

        var backBtn = UiHelper.NavButton(bg.transform, "\u2190 Back", () => { Office.CloseAllRooms(); Destroy(this); });
        var backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0f, 0.92f);
        backRect.anchorMax = new Vector2(0f, 0.92f);
        backRect.anchoredPosition = new Vector2(70f, -16f);
        backRect.sizeDelta = new Vector2(110f, 36f);

        FeedbackText = UiHelper.Label(bg.transform, string.Empty, 16,
            new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.09f), TextAnchor.LowerLeft);
        FeedbackText.color = new Color(0.6f, 0.9f, 0.6f);

        return bg;
    }

    protected abstract string RoomTitle();
    protected abstract void BuildContent(Transform root);

    protected void Feedback(string msg)
    {
        if (FeedbackText != null) FeedbackText.text = msg;
    }
}
