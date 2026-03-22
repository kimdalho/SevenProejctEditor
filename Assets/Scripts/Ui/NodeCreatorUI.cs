using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeCreatorUI : MonoBehaviour
{
    [Header("Style")]
    public TMP_FontAsset font;
    public float buttonWidth  = 140f;
    public float buttonHeight = 40f;
    public float spacing      = 8f;
    public float marginLeft   = 16f;
    public float marginTop    = 16f;

    [Header("Node Colors")]
    public Color sayColor       = new Color(0.25f, 0.55f, 1f);
    public Color fadeColor      = new Color(0.55f, 0.25f, 1f);
    public Color waitColor      = new Color(0.25f, 0.75f, 0.55f);
    public Color showCharColor  = new Color(1f,    0.65f, 0.2f);
    public Color choiceColor    = new Color(1f,    0.35f, 0.35f);

    public static NodeCreatorUI instance;

    public Color GetColor(eNodeType type) => type switch
    {
        eNodeType.Say           => sayColor,
        eNodeType.Fade          => fadeColor,
        eNodeType.Wait          => waitColor,
        eNodeType.ShowCharacter => showCharColor,
        eNodeType.Choice        => choiceColor,
        _                       => Color.white,
    };

    private void Awake()
    {
        instance = this;
        BuildUI();
    }

    private void BuildUI()
    {
        var entries = new (eNodeType type, string label)[]
        {
            (eNodeType.Say,           "Say"),
            (eNodeType.Fade,          "Fade"),
            (eNodeType.Wait,          "Wait"),
            (eNodeType.ShowCharacter, "Show Character"),
            (eNodeType.Choice,        "Choice"),
        };

        // ── Canvas ──────────────────────────────────────
        var canvasGo = new GameObject("NodeCreatorCanvas");
        canvasGo.transform.SetParent(transform);


        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGo.AddComponent<GraphicRaycaster>();

        // ── 패널 ────────────────────────────────────────
        var panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGo.transform, false);

        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 1);
        panelRT.anchorMax = new Vector2(0, 1);
        panelRT.pivot     = new Vector2(0, 1);
        panelRT.anchoredPosition = new Vector2(marginLeft, -marginTop);
        panelRT.sizeDelta = new Vector2(buttonWidth,
            entries.Length * buttonHeight + (entries.Length - 1) * spacing);

        var vlg = panel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing              = spacing;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth    = true;
        vlg.childControlHeight   = false;

        // ── 버튼 ────────────────────────────────────────
        foreach (var (type, label) in entries)
        {
            var btnGo = new GameObject(label);
            btnGo.transform.SetParent(panel.transform, false);

            var btnRT = btnGo.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            var img = btnGo.AddComponent<Image>();
            img.color = GetColor(type);

            var btn = btnGo.AddComponent<Button>();
            var captured = type;
            btn.onClick.AddListener(() => NodeGraphManager.instance.CreateNodeToType(captured));

            var cb = btn.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = cb;

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(btnGo.transform, false);

            var textRT = textGo.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 18f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            if (font != null) tmp.font = font;
        }

        // ── TSV 저장 버튼 (좌측 하단) ────────────────────
        var saveBtnGo = new GameObject("SaveButton");
        saveBtnGo.transform.SetParent(canvasGo.transform, false);

        var saveBtnRT = saveBtnGo.AddComponent<RectTransform>();
        saveBtnRT.anchorMin = new Vector2(0, 0);
        saveBtnRT.anchorMax = new Vector2(0, 0);
        saveBtnRT.pivot     = new Vector2(0, 0);
        saveBtnRT.anchoredPosition = new Vector2(marginLeft, marginTop);
        saveBtnRT.sizeDelta = new Vector2(buttonWidth, buttonHeight);

        saveBtnGo.AddComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);

        var saveBtn = saveBtnGo.AddComponent<Button>();
        saveBtn.onClick.AddListener(() => NodeGraphManager.instance.SaveTSV());

        var saveLabelGo = new GameObject("Label");
        saveLabelGo.transform.SetParent(saveBtnGo.transform, false);

        var saveLabelRT = saveLabelGo.AddComponent<RectTransform>();
        saveLabelRT.anchorMin = Vector2.zero;
        saveLabelRT.anchorMax = Vector2.one;
        saveLabelRT.offsetMin = saveLabelRT.offsetMax = Vector2.zero;

        var saveTmp = saveLabelGo.AddComponent<TextMeshProUGUI>();
        saveTmp.text      = "Save TSV";
        saveTmp.fontSize  = 18f;
        saveTmp.alignment = TextAlignmentOptions.Center;
        saveTmp.color     = Color.white;
        if (font != null) saveTmp.font = font;
    }
}
