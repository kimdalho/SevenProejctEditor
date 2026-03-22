using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class NodeEditorPanelCreator
{
    private const string FontPath = "Assets/TextMesh Pro/Fonts/Maplestory Light SDF.asset";

    [MenuItem("Tools/Node Graph/에디터 인터페이스 생성")]
    public static void Create()
    {
        var existing = Object.FindObjectOfType<NodeEditorPanel>();
        if (existing != null)
        {
            Selection.activeGameObject = existing.gameObject;
            Debug.Log("[NodeEditorPanel] 이미 존재합니다.");
            return;
        }

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

        // ── Canvas 루트 (inactive로 생성 → AddComponent 시 Awake 방지) ─────
        var root = new GameObject("NodeEditorPanel", typeof(RectTransform));
        root.SetActive(false);
        Undo.RegisterCreatedObjectUndo(root, "Create NodeEditorPanel");

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        root.AddComponent<GraphicRaycaster>();

        // ── 패널 배경 ─────────────────────────────────────
        var panel = UI(root.transform, "Panel");
        var panelRT = RT(panel);
        panelRT.anchorMin        = new Vector2(1f, 0.5f);
        panelRT.anchorMax        = new Vector2(1f, 0.5f);
        panelRT.pivot            = new Vector2(1f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta        = new Vector2(340f, 600f);
        panel.AddComponent<Image>().color = new Color(0.10f, 0.10f, 0.13f, 0.97f);

        // ── 헤더 ──────────────────────────────────────────
        var header = UI(panel.transform, "Header");
        var headerRT = RT(header);
        headerRT.anchorMin = new Vector2(0, 1); headerRT.anchorMax = new Vector2(1, 1);
        headerRT.pivot     = new Vector2(0.5f, 1);
        headerRT.anchoredPosition = Vector2.zero;
        headerRT.sizeDelta = new Vector2(0, 44);
        header.AddComponent<Image>().color = new Color(0.07f, 0.07f, 0.10f, 1f);

        var titleGo = UI(header.transform, "TitleText");
        Stretch(RT(titleGo), new Vector2(12, 0), new Vector2(-44, 0));
        var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Node Editor"; titleTmp.fontSize = 16;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
        titleTmp.color = Color.white; titleTmp.raycastTarget = false;
        if (font) titleTmp.font = font;

        var closeBtnGo = UI(header.transform, "CloseButton");
        var closeBtnRT = RT(closeBtnGo);
        closeBtnRT.anchorMin = new Vector2(1, 0); closeBtnRT.anchorMax = new Vector2(1, 1);
        closeBtnRT.pivot = new Vector2(1, 0.5f);
        closeBtnRT.anchoredPosition = Vector2.zero; closeBtnRT.sizeDelta = new Vector2(42, 0);
        closeBtnGo.AddComponent<Image>().color = new Color(0.65f, 0.15f, 0.15f, 1f);
        var closeBtn = closeBtnGo.AddComponent<Button>();

        var xGo = UI(closeBtnGo.transform, "X");
        Stretch(RT(xGo));
        var xTmp = xGo.AddComponent<TextMeshProUGUI>();
        xTmp.text = "×"; xTmp.fontSize = 22;
        xTmp.alignment = TextAlignmentOptions.Center;
        xTmp.color = Color.white; xTmp.raycastTarget = false;
        if (font) xTmp.font = font;

        // ── ScrollView ────────────────────────────────────
        var scrollGo = UI(panel.transform, "ScrollView");
        var scrollRT = RT(scrollGo);
        scrollRT.anchorMin = Vector2.zero; scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = Vector2.zero; scrollRT.offsetMax = new Vector2(0, -44);
        var scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        var vpGo = UI(scrollGo.transform, "Viewport");
        Stretch(RT(vpGo));
        vpGo.AddComponent<RectMask2D>();
        scrollRect.viewport = RT(vpGo);

        var contentGo = UI(vpGo.transform, "Content");
        var contentRT = RT(contentGo);
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero; contentRT.sizeDelta = Vector2.zero;
        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 4; vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.childForceExpandWidth = true;  vlg.childForceExpandHeight = false;
        vlg.childControlWidth     = true;  vlg.childControlHeight     = false;
        contentGo.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        // ── 타입별 그룹 빌드 (로컬 변수로 저장) ──────────────
        var sayGroup      = BuildGroup(contentGo.transform, "Group_Say",
                                       new Color(0.25f, 0.55f, 1f, 0.12f), "Say", font);
        var sayCharItem   = BuildItem(sayGroup.transform, "Item_character", font);
        var sayStr1Item   = BuildItem(sayGroup.transform, "Item_str1",      font);

        var showCharGroup = BuildGroup(contentGo.transform, "Group_ShowCharacter",
                                       new Color(1f, 0.65f, 0.2f, 0.12f), "ShowCharacter", font);
        var scCharItem    = BuildItem(showCharGroup.transform, "Item_character",   font);
        var scExprItem    = BuildItem(showCharGroup.transform, "Item_expression",  font);
        var scLocItem     = BuildItem(showCharGroup.transform, "Item_endLocation", font);
        var scEaseItem    = BuildItem(showCharGroup.transform, "Item_easeType",    font);
        var scWaitItem    = BuildItem(showCharGroup.transform, "Item_wait",        font);

        var fadeGroup     = BuildGroup(contentGo.transform, "Group_Fade",
                                       new Color(0.55f, 0.25f, 1f, 0.12f), "Fade", font);
        var fadeDirItem   = BuildItem(fadeGroup.transform, "Item_direction", font);
        var fadeEaseItem  = BuildItem(fadeGroup.transform, "Item_easeType",  font);
        var fadeDurItem   = BuildItem(fadeGroup.transform, "Item_duration",  font);

        var waitGroup     = BuildGroup(contentGo.transform, "Group_Wait",
                                       new Color(0.25f, 0.75f, 0.55f, 0.12f), "Wait", font);
        var waitDurItem   = BuildItem(waitGroup.transform, "Item_wait", font);

        var choiceGroup     = BuildGroup(contentGo.transform, "Group_Choice",
                                         new Color(1f, 0.35f, 0.35f, 0.12f), "Choice", font);
        var choiceCountItem = BuildItem(choiceGroup.transform, "Item_count", font);
        var choiceOptItems  = new NodeEditorListItem[4];
        var choiceTargItems = new NodeEditorListItem[4];
        for (int i = 0; i < 4; i++)
        {
            choiceOptItems[i]  = BuildItem(choiceGroup.transform, $"Item_option{i+1}", font);
            choiceTargItems[i] = BuildItem(choiceGroup.transform, $"Item_target{i+1}", font);
        }

        // ── NodeEditorPanel 스크립트 추가 (inactive이므로 Awake 실행 안 됨) ──
        var panelScript = root.AddComponent<NodeEditorPanel>();

        // ── 모든 필드 한꺼번에 연결 ────────────────────────
        panelScript.titleText   = titleTmp;
        panelScript.closeButton = closeBtn;

        panelScript.sayGroup      = sayGroup;
        panelScript.sayCharItem   = sayCharItem;
        panelScript.sayStr1Item   = sayStr1Item;

        panelScript.showCharGroup = showCharGroup;
        panelScript.scCharItem    = scCharItem;
        panelScript.scExprItem    = scExprItem;
        panelScript.scLocItem     = scLocItem;
        panelScript.scEaseItem    = scEaseItem;
        panelScript.scWaitItem    = scWaitItem;

        panelScript.fadeGroup    = fadeGroup;
        panelScript.fadeDirItem  = fadeDirItem;
        panelScript.fadeEaseItem = fadeEaseItem;
        panelScript.fadeDurItem  = fadeDurItem;

        panelScript.waitGroup   = waitGroup;
        panelScript.waitDurItem = waitDurItem;

        panelScript.choiceGroup     = choiceGroup;
        panelScript.choiceCountItem = choiceCountItem;
        panelScript.choiceOptItems  = choiceOptItems;
        panelScript.choiceTargItems = choiceTargItems;

        // ── 씬에서 보이도록 active로 전환 ────────────────────
        root.SetActive(true);

        EditorUtility.SetDirty(root);
        Selection.activeGameObject = root;
        Debug.Log("[NodeEditorPanel] 씬 배치 완료.");
    }

    // ── 그룹 컨테이너 ─────────────────────────────────────

    private static GameObject BuildGroup(Transform parent, string name,
        Color bgColor, string label, TMP_FontAsset font)
    {
        var go = UI(parent, name);
        var rt = RT(go);
        rt.sizeDelta = Vector2.zero;
        go.AddComponent<Image>().color = bgColor;

        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 3; vlg.padding = new RectOffset(0, 0, 0, 4);
        vlg.childForceExpandWidth = true;  vlg.childForceExpandHeight = false;
        vlg.childControlWidth     = true;  vlg.childControlHeight     = false;
        go.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        // 섹션 타이틀
        var hdr = UI(go.transform, "SectionHeader");
        RT(hdr).sizeDelta = new Vector2(0, 26);
        hdr.AddComponent<Image>().color = new Color(0, 0, 0, 0.35f);
        hdr.AddComponent<LayoutElement>().preferredHeight = 26;

        var hdrTxtGo = UI(hdr.transform, "Label");
        Stretch(RT(hdrTxtGo));
        var hdrTmp = hdrTxtGo.AddComponent<TextMeshProUGUI>();
        hdrTmp.text = label; hdrTmp.fontSize = 13;
        hdrTmp.fontStyle = FontStyles.Bold;
        hdrTmp.alignment = TextAlignmentOptions.MidlineLeft;
        hdrTmp.color = new Color(1f, 1f, 1f, 0.75f);
        hdrTmp.margin = new Vector4(8, 0, 0, 0);
        hdrTmp.raycastTarget = false;
        if (font) hdrTmp.font = font;

        return go;
    }

    // ── 리스트 아이템 ─────────────────────────────────────

    private static NodeEditorListItem BuildItem(Transform parent, string name, TMP_FontAsset font)
    {
        // 루트 행
        var root = UI(parent, name);
        root.AddComponent<Image>().color = new Color(0.14f, 0.14f, 0.18f, 1f);
        var le = root.AddComponent<LayoutElement>();
        le.preferredHeight = 52f;
        RT(root).sizeDelta = new Vector2(0, 52);

        var item = root.AddComponent<NodeEditorListItem>();

        // 필드명 라벨 (위 20px)
        var lblGo = UI(root.transform, "Label");
        var lblRT = RT(lblGo);
        lblRT.anchorMin = new Vector2(0, 1); lblRT.anchorMax = new Vector2(1, 1);
        lblRT.pivot = new Vector2(0.5f, 1);
        lblRT.anchoredPosition = Vector2.zero; lblRT.sizeDelta = new Vector2(0, 20);
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text = name; lblTmp.fontSize = 11;
        lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
        lblTmp.color = new Color(0.68f, 0.68f, 0.68f);
        lblTmp.margin = new Vector4(6, 0, 6, 0);
        lblTmp.raycastTarget = false;
        if (font) lblTmp.font = font;
        item.labelText = lblTmp;

        // 컨트롤 공통 영역
        var ctrl = UI(root.transform, "ControlArea");
        var ctrlRT = RT(ctrl);
        ctrlRT.anchorMin = Vector2.zero; ctrlRT.anchorMax = new Vector2(1, 1);
        ctrlRT.offsetMin = new Vector2(6, 4); ctrlRT.offsetMax = new Vector2(-6, -20);

        // InputSection
        item.inputSection = BuildInputSection(ctrl.transform, font);
        Stretch(RT(item.inputSection));
        item.inputField = item.inputSection.GetComponentInChildren<TMP_InputField>();

        // EnumSection
        item.enumSection = BuildEnumSection(ctrl.transform, font);
        Stretch(RT(item.enumSection));
        item.dropdown = item.enumSection.GetComponent<TMP_Dropdown>();
        item.enumSection.SetActive(false);

        // ReadOnlySection
        item.readOnlySection = BuildReadOnlySection(ctrl.transform, font);
        Stretch(RT(item.readOnlySection));
        item.readOnlyLabel = item.readOnlySection.GetComponentInChildren<TextMeshProUGUI>();
        item.readOnlySection.SetActive(false);

        return item;
    }

    // ── 섹션 빌더 ─────────────────────────────────────────

    private static GameObject BuildInputSection(Transform parent, TMP_FontAsset font)
    {
        var root = UI(parent, "InputSection");
        root.AddComponent<Image>().color = new Color(0.11f, 0.11f, 0.15f, 1f);

        var ta = UI(root.transform, "TextArea");
        var taRT = RT(ta);
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(4, 2); taRT.offsetMax = new Vector2(-4, -2);
        ta.AddComponent<RectMask2D>();

        var textGo = UI(ta.transform, "Text");
        Stretch(RT(textGo));
        var textTmp = textGo.AddComponent<TextMeshProUGUI>();
        textTmp.fontSize = 13; textTmp.color = Color.white;
        textTmp.enableWordWrapping = false;
        textTmp.overflowMode = TextOverflowModes.Overflow;
        if (font) textTmp.font = font;

        var phGo = UI(ta.transform, "Placeholder");
        Stretch(RT(phGo));
        var phTmp = phGo.AddComponent<TextMeshProUGUI>();
        phTmp.fontSize = 13; phTmp.color = new Color(1, 1, 1, 0.25f);
        phTmp.text = "..."; phTmp.fontStyle = FontStyles.Italic;
        phTmp.overflowMode = TextOverflowModes.Overflow;
        if (font) phTmp.font = font;

        var input = root.AddComponent<TMP_InputField>();
        input.textViewport = taRT;
        input.textComponent = textTmp;
        input.placeholder = phTmp;
        input.pointSize = 13;
        return root;
    }

    private static GameObject BuildEnumSection(Transform parent, TMP_FontAsset font)
    {
        var root = UI(parent, "EnumSection");
        root.AddComponent<Image>().color = new Color(0.18f, 0.22f, 0.30f, 1f);

        var dd = root.AddComponent<TMP_Dropdown>();

        // 드롭다운 라벨 (선택된 값 표시)
        var labelGo = UI(root.transform, "Label");
        Stretch(RT(labelGo), new Vector2(8, 2), new Vector2(-28, -2));
        var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
        labelTmp.fontSize = 13;
        labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
        labelTmp.color = new Color(1f, 0.85f, 0.35f);
        if (font) labelTmp.font = font;
        dd.captionText = labelTmp;

        // 화살표
        var arrowGo = UI(root.transform, "Arrow");
        var arrowRT = RT(arrowGo);
        arrowRT.anchorMin = new Vector2(1, 0); arrowRT.anchorMax = new Vector2(1, 1);
        arrowRT.pivot = new Vector2(1, 0.5f);
        arrowRT.anchoredPosition = Vector2.zero; arrowRT.sizeDelta = new Vector2(24, 0);
        var arrowTmp = arrowGo.AddComponent<TextMeshProUGUI>();
        arrowTmp.text = "▼"; arrowTmp.fontSize = 11;
        arrowTmp.alignment = TextAlignmentOptions.Center;
        arrowTmp.color = new Color(1f, 0.85f, 0.35f);
        arrowTmp.raycastTarget = false;
        if (font) arrowTmp.font = font;

        // 템플릿 (드롭다운 목록)
        var templateGo = UI(root.transform, "Template");
        var templateRT = RT(templateGo);
        templateRT.anchorMin = new Vector2(0, 0); templateRT.anchorMax = new Vector2(1, 0);
        templateRT.pivot = new Vector2(0.5f, 1);
        templateRT.anchoredPosition = Vector2.zero; templateRT.sizeDelta = new Vector2(0, 150);
        templateGo.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.20f, 1f);
        var templateScroll = templateGo.AddComponent<ScrollRect>();
        templateScroll.horizontal = false;
        templateGo.AddComponent<RectMask2D>();

        var vpGo = UI(templateGo.transform, "Viewport");
        Stretch(RT(vpGo));
        templateScroll.viewport = RT(vpGo);

        var contentGo = UI(vpGo.transform, "Content");
        var contentRT = RT(contentGo);
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero; contentRT.sizeDelta = Vector2.zero;
        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;     vlg.childControlHeight = true;
        contentGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        templateScroll.content = contentRT;

        // 아이템 템플릿
        var itemGo = UI(contentGo.transform, "Item");
        RT(itemGo).sizeDelta = new Vector2(0, 28);
        itemGo.AddComponent<Image>().color = new Color(0.18f, 0.22f, 0.30f, 1f);
        var itemToggle = itemGo.AddComponent<Toggle>();

        var itemBgGo = UI(itemGo.transform, "Item Background");
        Stretch(RT(itemBgGo));
        itemBgGo.AddComponent<Image>().color = new Color(0.26f, 0.32f, 0.42f, 1f);
        itemToggle.targetGraphic = itemBgGo.GetComponent<Image>();

        var itemCheckGo = UI(itemGo.transform, "Item Checkmark");
        var itemCheckRT = RT(itemCheckGo);
        itemCheckRT.anchorMin = new Vector2(0, 0.5f); itemCheckRT.anchorMax = new Vector2(0, 0.5f);
        itemCheckRT.sizeDelta = new Vector2(16, 16); itemCheckRT.anchoredPosition = new Vector2(8, 0);
        var checkImg = itemCheckGo.AddComponent<Image>();
        checkImg.color = new Color(1f, 0.85f, 0.35f);
        itemToggle.graphic = checkImg;

        var itemLabelGo = UI(itemGo.transform, "Item Label");
        Stretch(RT(itemLabelGo), new Vector2(20, 2), new Vector2(-4, -2));
        var itemLabelTmp = itemLabelGo.AddComponent<TextMeshProUGUI>();
        itemLabelTmp.fontSize = 13;
        itemLabelTmp.alignment = TextAlignmentOptions.MidlineLeft;
        itemLabelTmp.color = Color.white;
        if (font) itemLabelTmp.font = font;

        dd.itemText = itemLabelTmp;
        dd.template = templateRT;
        templateGo.SetActive(false);

        return root;
    }

    private static GameObject BuildReadOnlySection(Transform parent, TMP_FontAsset font)
    {
        var root = UI(parent, "ReadOnlySection");

        var lblGo = UI(root.transform, "ValueText");
        Stretch(RT(lblGo));
        var tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "—"; tmp.fontSize = 13;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = new Color(0.5f, 0.9f, 1f);
        tmp.margin = new Vector4(4, 0, 4, 0);
        tmp.raycastTarget = false;
        if (font) tmp.font = font;

        return root;
    }

    // ── 헬퍼 ──────────────────────────────────────────────

    /// RectTransform을 가진 UI GameObject 생성
    private static GameObject UI(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static RectTransform RT(GameObject go) =>
        go.GetComponent<RectTransform>();

    private static void Stretch(RectTransform rt,
        Vector2 offsetMin = default, Vector2 offsetMax = default)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;   rt.offsetMax = offsetMax;
    }
}
