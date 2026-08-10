using UnityEngine;
using UnityEngine.UI;

public static class UiHelper
{
    private static Font _font;
    private static Font Font => _font ??= Resources.GetBuiltinResource<Font>("Arial.ttf");

    public static GameObject Canvas(Transform parent, string name, int sortOrder = 0)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortOrder;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    public static GameObject Panel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        Anchor(go, anchorMin, anchorMax);
        return go;
    }

    public static Text Label(Transform parent, string text, int size,
        Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        var go = new GameObject("Label_" + text.Replace(" ", "").Substring(0, System.Math.Min(12, text.Length)));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Font;
        t.fontSize = size;
        t.alignment = alignment;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        Anchor(go, anchorMin, anchorMax);
        return t;
    }

    public static Button NavButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var btn = go.AddComponent<Button>();
        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.38f, 0.65f, 1f);
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(130f, 44f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var t = textGO.AddComponent<Text>();
        t.text = label;
        t.font = Font;
        t.fontSize = 17;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
        return btn;
    }

    public static Button ActionButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick,
        Color? color = null)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var btn = go.AddComponent<Button>();
        var img = go.AddComponent<Image>();
        img.color = color ?? new Color(0.24f, 0.48f, 0.22f, 1f);
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(200f, 40f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var t = textGO.AddComponent<Text>();
        t.text = label;
        t.font = Font;
        t.fontSize = 16;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
        return btn;
    }

    public static GameObject Row(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, float spacing = 10f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childForceExpandWidth = false;
        layout.childControlWidth = false;
        layout.childAlignment = TextAnchor.MiddleCenter;
        Anchor(go, anchorMin, anchorMax);
        return go;
    }

    public static GameObject Column(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, float spacing = 8f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;
        Anchor(go, anchorMin, anchorMax);
        return go;
    }

    private static void Anchor(GameObject go, Vector2 min, Vector2 max)
    {
        var r = go.GetComponent<RectTransform>();
        if (r == null) r = go.AddComponent<RectTransform>();
        r.anchorMin = min;
        r.anchorMax = max;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}
