#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메뉴에서 CutScene Canvas 프리팹 구조를 씬에 자동 생성하는 에디터 도구.
/// 생성 후 디자인 수정 → 프리팹으로 저장하여 NodePlayer / CutScenePlayer에 연결.
/// </summary>
public static class CutSceneUICreator
{
    [MenuItem("Tools/CutScene/Create CutScene Canvas Prefab")]
    public static void CreateCutSceneCanvas()
    {
        // ── Canvas ──
        var canvasObj = new GameObject("CutSceneCanvas");
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create CutScene Canvas");

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // ══════════════════════════════════════════════
        // 1) CharacterLayer (맨 뒤)
        // ══════════════════════════════════════════════
        var charLayerObj = CreateFullScreenRect(canvasObj.transform, "CharacterLayer");
        var charDisplay = charLayerObj.AddComponent<CharacterDisplay>();
        // CharacterDisplay.Awake()에서 layerRect를 자동 할당하므로 별도 설정 불필요

        // ══════════════════════════════════════════════
        // 2) DialoguePanel (중간)
        // ══════════════════════════════════════════════
        var dialoguePanelObj = CreateFullScreenRect(canvasObj.transform, "DialoguePanel");
        var canvasGroup = dialoguePanelObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        var dialogueUI = dialoguePanelObj.AddComponent<DialogueUI>();

        // canvasGroup 연결
        var dialogueSO = new SerializedObject(dialogueUI);
        dialogueSO.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;

        // ── DialogueBox (하단 28%) ──
        var boxObj = new GameObject("DialogueBox");
        boxObj.transform.SetParent(dialoguePanelObj.transform, false);
        var boxRect = boxObj.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0f, 0f);
        boxRect.anchorMax = new Vector2(1f, 0.28f);
        boxRect.offsetMin = new Vector2(40f, 20f);
        boxRect.offsetMax = new Vector2(-40f, 0f);

        var dialogueBoxBG = boxObj.AddComponent<Image>();
        dialogueBoxBG.color = new Color(0f, 0f, 0f, 0.75f);

        var boxOutline = boxObj.AddComponent<Outline>();
        boxOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        boxOutline.effectDistance = new Vector2(2f, 2f);

        // ── Dialogue_TMP (대화 상자 안) ──
        var textObj = new GameObject("Dialogue_TMP");
        textObj.transform.SetParent(boxObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(30f, 20f);
        textRect.offsetMax = new Vector2(-30f, -12f);
        var dialogueTMP = textObj.AddComponent<TextMeshProUGUI>();
        dialogueTMP.fontSize = 28f;
        dialogueTMP.lineSpacing = 8f;
        dialogueTMP.alignment = TextAlignmentOptions.TopLeft;
        dialogueTMP.color = Color.white;
        dialogueTMP.overflowMode = TextOverflowModes.Ellipsis;
        dialogueTMP.text = "대사 텍스트 미리보기...";

        // ── NextIndicator ▼ ──
        var indicatorObj = new GameObject("NextIndicator");
        indicatorObj.transform.SetParent(boxObj.transform, false);
        var indRect = indicatorObj.AddComponent<RectTransform>();
        indRect.anchorMin = new Vector2(1f, 0f);
        indRect.anchorMax = new Vector2(1f, 0f);
        indRect.pivot = new Vector2(1f, 0f);
        indRect.anchoredPosition = new Vector2(-20f, 12f);
        indRect.sizeDelta = new Vector2(40f, 30f);
        var nextIndicator = indicatorObj.AddComponent<TextMeshProUGUI>();
        nextIndicator.text = "\u25bc";
        nextIndicator.fontSize = 22f;
        nextIndicator.alignment = TextAlignmentOptions.BottomRight;
        nextIndicator.color = new Color(1f, 1f, 1f, 0.6f);
        indicatorObj.SetActive(false);

        // ── NameTag (대화 상자 위) ──
        var nameTagObj = new GameObject("NameTag");
        nameTagObj.transform.SetParent(dialoguePanelObj.transform, false);
        var nameTagRect = nameTagObj.AddComponent<RectTransform>();
        nameTagRect.anchorMin = new Vector2(0f, 0.28f);
        nameTagRect.anchorMax = new Vector2(0f, 0.28f);
        nameTagRect.pivot = new Vector2(0f, 0f);
        nameTagRect.anchoredPosition = new Vector2(60f, 4f);
        nameTagRect.sizeDelta = new Vector2(260f, 48f);

        var nameTagBG = nameTagObj.AddComponent<Image>();
        nameTagBG.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);

        var nameTagOutline = nameTagObj.AddComponent<Outline>();
        nameTagOutline.effectColor = new Color(1f, 0.85f, 0.4f, 0.4f);
        nameTagOutline.effectDistance = new Vector2(1f, 1f);

        // ── CharacterName_TMP ──
        var nameObj = new GameObject("CharacterName_TMP");
        nameObj.transform.SetParent(nameTagObj.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = Vector2.zero;
        nameRect.anchorMax = Vector2.one;
        nameRect.offsetMin = new Vector2(16f, 0f);
        nameRect.offsetMax = new Vector2(-16f, 0f);
        var characterNameTMP = nameObj.AddComponent<TextMeshProUGUI>();
        characterNameTMP.fontSize = 28f;
        characterNameTMP.fontStyle = FontStyles.Bold;
        characterNameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        characterNameTMP.color = new Color(1f, 0.9f, 0.5f);
        characterNameTMP.text = "캐릭터 이름";

        // DialogueUI SerializedObject로 필드 연결
        dialogueSO.FindProperty("characterNameTMP").objectReferenceValue = characterNameTMP;
        dialogueSO.FindProperty("dialogueTMP").objectReferenceValue = dialogueTMP;
        dialogueSO.FindProperty("dialogueBoxBG").objectReferenceValue = dialogueBoxBG;
        dialogueSO.FindProperty("nameTagBG").objectReferenceValue = nameTagBG;
        dialogueSO.FindProperty("nextIndicator").objectReferenceValue = nextIndicator;
        dialogueSO.ApplyModifiedPropertiesWithoutUndo();

        // ══════════════════════════════════════════════
        // 3) FadePanel (맨 앞)
        // ══════════════════════════════════════════════
        var fadePanelObj = CreateFullScreenRect(canvasObj.transform, "FadePanel");
        var fadeUI = fadePanelObj.AddComponent<FadeUI>();

        var fadeImgObj = new GameObject("FadeImage");
        fadeImgObj.transform.SetParent(fadePanelObj.transform, false);
        var fadeImgRect = fadeImgObj.AddComponent<RectTransform>();
        fadeImgRect.anchorMin = Vector2.zero;
        fadeImgRect.anchorMax = Vector2.one;
        fadeImgRect.offsetMin = Vector2.zero;
        fadeImgRect.offsetMax = Vector2.zero;
        var fadeImage = fadeImgObj.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;

        var fadeSO = new SerializedObject(fadeUI);
        fadeSO.FindProperty("fadeImage").objectReferenceValue = fadeImage;
        fadeSO.ApplyModifiedPropertiesWithoutUndo();

        // ══════════════════════════════════════════════
        // 4) DebugInfo (진행 표시, CutScenePlayer 전용)
        // ══════════════════════════════════════════════
        var debugObj = new GameObject("DebugInfo");
        debugObj.transform.SetParent(canvasObj.transform, false);
        var debugRect = debugObj.AddComponent<RectTransform>();
        debugRect.anchorMin = Vector2.zero;
        debugRect.anchorMax = Vector2.one;
        debugRect.offsetMin = Vector2.zero;
        debugRect.offsetMax = Vector2.zero;

        // Progress_TMP (좌측 상단)
        CreateDebugTMP(debugObj.transform, "Progress_TMP",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -15f), new Vector2(300f, 40f),
            24f, new Color(0.5f, 1f, 0.5f), TextAlignmentOptions.TopLeft,
            "[1 / 10]");

        // CommandInfo_TMP
        CreateDebugTMP(debugObj.transform, "CommandInfo_TMP",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -50f), new Vector2(600f, 30f),
            18f, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.TopLeft,
            "say [캐릭터] 대사 미리보기...");

        // Hint_TMP (우측 상단)
        CreateDebugTMP(debugObj.transform, "Hint_TMP",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -15f), new Vector2(250f, 30f),
            18f, new Color(1f, 1f, 1f, 0.4f), TextAlignmentOptions.TopRight,
            "ESC: 에디터로 복귀");

        // 선택
        Selection.activeGameObject = canvasObj;
        Debug.Log("[CutSceneUICreator] CutSceneCanvas가 씬에 생성되었습니다. 디자인 수정 후 프리팹으로 저장하세요.");
    }

    private static GameObject CreateFullScreenRect(Transform parent, string name)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return obj;
    }

    private static void CreateDebugTMP(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 sizeDelta,
        float fontSize, Color color, TextAlignmentOptions align,
        string previewText)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        tmp.text = previewText;
    }
}
#endif
