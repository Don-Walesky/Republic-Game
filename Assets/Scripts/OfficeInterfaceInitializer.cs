using UnityEngine;

public class OfficeInterfaceInitializer : MonoBehaviour
{
    private void Start()
    {
        var host = new GameObject("OfficeInterfaceHost");
        host.transform.SetParent(transform, false);

        var canvasGO = new GameObject("OfficeInterfaceCanvas");
        canvasGO.transform.SetParent(host.transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var panelGO = new GameObject("OfficeInterfacePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.14f, 1f);

        var rect = panelGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.04f, 0.06f);
        rect.anchorMax = new Vector2(0.96f, 0.94f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var title = new GameObject("Title");
        title.transform.SetParent(panelGO.transform, false);
        var titleText = title.AddComponent<Text>();
        titleText.text = "Executive Command Center";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 28;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.88f);
        titleRect.anchorMax = new Vector2(0.95f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        var buttonHolder = new GameObject("Buttons");
        buttonHolder.transform.SetParent(panelGO.transform, false);
        var buttonRect = buttonHolder.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.05f, 0.7f);
        buttonRect.anchorMax = new Vector2(0.95f, 0.85f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        var layout = buttonHolder.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childForceExpandWidth = false;
        layout.childControlWidth = true;

        CreateButton(buttonHolder.transform, "Military", out var militaryButton);
        CreateButton(buttonHolder.transform, "Ministries", out var ministriesButton);
        CreateButton(buttonHolder.transform, "Legislature", out var legislatureButton);
        CreateButton(buttonHolder.transform, "Diplomacy", out var diplomacyButton);
        CreateButton(buttonHolder.transform, "Press", out var pressButton);

        var content = new GameObject("Content");
        content.transform.SetParent(panelGO.transform, false);
        var contentText = content.AddComponent<Text>();
        contentText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        contentText.fontSize = 20;
        contentText.color = Color.white;
        contentText.alignment = TextAnchor.UpperLeft;
        contentText.horizontalOverflow = HorizontalWrapMode.Wrap;
        contentText.verticalOverflow = VerticalWrapMode.Overflow;
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.05f, 0.1f);
        contentRect.anchorMax = new Vector2(0.95f, 0.68f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        var controller = host.AddComponent<OfficeInterfaceController>();
        controller.militaryButton = militaryButton;
        controller.ministriesButton = ministriesButton;
        controller.legislatureButton = legislatureButton;
        controller.diplomacyButton = diplomacyButton;
        controller.pressButton = pressButton;
        controller.contentText = contentText;
    }

    private static void CreateButton(Transform parent, string label, out Button button)
    {
        var buttonGO = new GameObject(label.Replace(" ", string.Empty));
        buttonGO.transform.SetParent(parent, false);
        button = buttonGO.AddComponent<Button>();
        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.7f, 1f);
        button.targetGraphic = image;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        var text = textGO.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        var rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 42f);
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}
