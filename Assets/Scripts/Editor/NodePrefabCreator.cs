#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tools > CutScene > Create Node Reference Prefabs
/// 4가지 노드 타입(Say, Fade, ShowCharacter, Wait)의 레이아웃 참조 프리팹을 씬에 생성.
/// 생성 후 확인 → 각 기존 프리팹에 동일 구조로 적용.
/// </summary>
public static class NodePrefabCreator
{
    // ── 공통 상수 ──────────────────────────────────────
    private const float CONTAINER_SCALE = 0.1f;
    private const float HEADER_FONT = 0.25f;
    private const float LABEL_FONT = 0.22f;
    private const float INPUT_FONT = 0.22f;
    private const float INPUT_HEIGHT = 0.3f;
    private const float MULTILINE_HEIGHT = 0.6f;
    private const float LABEL_HEIGHT = 0.26f;
    private const float LAYOUT_SPACING = -0.02f;

    // ── 노드별 색상 ──────────────────────────────────────
    private static readonly Color SAY_COLOR = Color.white;
    private static readonly Color FADE_COLOR = new Color(1f, 0.6f, 0.6f);
    private static readonly Color SHOWCHAR_COLOR = new Color(0.6f, 1f, 0.6f);
    private static readonly Color WAIT_COLOR = new Color(1f, 0.8f, 0.4f);
    private static readonly Color CHOICE_COLOR = new Color(0.7f, 0.5f, 1f);

    // ══════════════════════════════════════════════════════
    // 메뉴: 4가지 노드 모두 생성
    // ══════════════════════════════════════════════════════
    [MenuItem("Tools/CutScene/Create Node Reference Prefabs")]
    public static void CreateAllNodeReferences()
    {
        float spacing = 2f;
        var say = CreateSayNode(new Vector3(0, 0, 0));
        var fade = CreateFadeNode(new Vector3(spacing, 0, 0));
        var showChar = CreateShowCharacterNode(new Vector3(spacing * 2, 0, 0));
        var wait = CreateWaitNode(new Vector3(spacing * 3, 0, 0));
        var choice = CreateChoiceNode(new Vector3(spacing * 4, 0, 0));

        // 전체 그룹
        var root = new GameObject("=== Node Reference Prefabs ===");
        Undo.RegisterCreatedObjectUndo(root, "Create Node References");
        say.transform.SetParent(root.transform);
        fade.transform.SetParent(root.transform);
        showChar.transform.SetParent(root.transform);
        wait.transform.SetParent(root.transform);
        choice.transform.SetParent(root.transform);

        Selection.activeGameObject = root;
        Debug.Log("[NodePrefabCreator] 5가지 노드 참조 프리팹이 씬에 생성되었습니다.\n" +
                  "각 노드의 구조를 확인한 후 기존 프리팹에 동일하게 적용하세요.");
    }

    // ══════════════════════════════════════════════════════
    // Say 노드
    // ══════════════════════════════════════════════════════
    [MenuItem("Tools/CutScene/Create Say Node Reference")]
    public static GameObject CreateSayNode(Vector3 pos = default)
    {
        var node = CreateNodeBase("Ref_Say", pos, SAY_COLOR);
        var canvas = node.GetComponentInChildren<Canvas>();
        var header = canvas.transform.Find("Header");

        // NodeContent 영역
        var content = CreateContentContainer(canvas.transform);

        // 캐릭터 이름 InputField
        CreateInputFieldPreview(content, "input_char", "유진", "캐릭터", INPUT_HEIGHT, false);

        // 대사 InputField (멀티라인)
        CreateInputFieldPreview(content, "input_str1",
            "안녕하세요! 오늘 날씨가\n좋네요...", "대사 내용...", MULTILINE_HEIGHT, true);

        return node;
    }

    // ══════════════════════════════════════════════════════
    // Fade 노드
    // ══════════════════════════════════════════════════════
    [MenuItem("Tools/CutScene/Create Fade Node Reference")]
    public static GameObject CreateFadeNode(Vector3 pos = default)
    {
        var node = CreateNodeBase("Ref_Fade", pos, FADE_COLOR);
        var canvas = node.GetComponentInChildren<Canvas>();

        var content = CreateContentContainer(canvas.transform);

        // Dir: FadeOut (클릭 토글 라벨)
        CreateEnumLabelPreview(content, "lbl_dir", "Dir: FadeOut",
            new Color(1f, 0.6f, 0.6f));

        // Ease: Linear (클릭 순환 라벨)
        CreateEnumLabelPreview(content, "lbl_ease", "Ease: Linear",
            new Color(1f, 0.85f, 0.3f));

        // 시간 InputField
        CreateInputFieldPreview(content, "input_dur", "1.0", "시간(초)", INPUT_HEIGHT, false);

        return node;
    }

    // ══════════════════════════════════════════════════════
    // ShowCharacter 노드
    // ══════════════════════════════════════════════════════
    [MenuItem("Tools/CutScene/Create ShowCharacter Node Reference")]
    public static GameObject CreateShowCharacterNode(Vector3 pos = default)
    {
        var node = CreateNodeBase("Ref_ShowCharacter", pos, SHOWCHAR_COLOR);
        var canvas = node.GetComponentInChildren<Canvas>();

        var content = CreateContentContainer(canvas.transform);

        // 캐릭터 이름 InputField
        CreateInputFieldPreview(content, "input_char", "유진", "캐릭터", INPUT_HEIGHT, false);

        // Exp: Happy (클릭 순환)
        CreateEnumLabelPreview(content, "lbl_exp", "Exp: Happy",
            new Color(1f, 0.85f, 0.3f));

        // Pos: Left (클릭 순환)
        CreateEnumLabelPreview(content, "lbl_to", "Pos: Left",
            new Color(0.5f, 0.8f, 1f));

        // Ease: OutCubic (클릭 순환)
        CreateEnumLabelPreview(content, "lbl_ease", "Ease: OutCubic",
            new Color(1f, 0.6f, 0.6f));

        // 시간 InputField
        CreateInputFieldPreview(content, "input_dur", "0.5", "시간(초)", INPUT_HEIGHT, false);

        return node;
    }

    // ══════════════════════════════════════════════════════
    // Wait 노드
    // ══════════════════════════════════════════════════════
    [MenuItem("Tools/CutScene/Create Wait Node Reference")]
    public static GameObject CreateWaitNode(Vector3 pos = default)
    {
        var node = CreateNodeBase("Ref_Wait", pos, WAIT_COLOR);
        var canvas = node.GetComponentInChildren<Canvas>();

        var content = CreateContentContainer(canvas.transform);

        // 대기 시간 InputField
        CreateInputFieldPreview(content, "input_wait", "2.5", "대기 시간(초)", INPUT_HEIGHT, false);

        return node;
    }

    // ══════════════════════════════════════════════════════
    // Choice 노드
    // ══════════════════════════════════════════════════════
    [MenuItem("Tools/CutScene/Create Choice Node Reference")]
    public static GameObject CreateChoiceNode(Vector3 pos = default)
    {
        // Choice는 Next 버튼 대신 branchButton 4개 사용 → 커스텀 생성
        var root = new GameObject("Ref_Choice");
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var sr = root.AddComponent<SpriteRenderer>();
        sr.color = CHOICE_COLOR;

        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.25f, 0.52f, 0.01f);
        col.center = new Vector3(0f, -0.06f, 0f);

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // ── Prev 버튼 ──
        var prevObj = new GameObject("Prev");
        prevObj.transform.SetParent(root.transform, false);
        prevObj.transform.localPosition = new Vector3(-0.05f, 0f, -0.21f);
        prevObj.transform.localScale = new Vector3(0.06757f, 0.06757f, 1f);
        var prevSR = prevObj.AddComponent<SpriteRenderer>();
        prevSR.color = new Color(0.3f, 0.8f, 0.3f);
        prevObj.AddComponent<BoxCollider>();
        var prevRB = prevObj.AddComponent<Rigidbody>();
        prevRB.isKinematic = true;
        prevRB.useGravity = false;

        // ── Branch 버튼 4개 (Next0~3, 오른쪽 세로 배치) ──
        for (int i = 0; i < 4; i++)
        {
            var branchObj = new GameObject($"Next{i}");
            branchObj.transform.SetParent(root.transform, false);
            branchObj.transform.localPosition = new Vector3(0.14f, 0f, -0.05f - i * 0.08f);
            branchObj.transform.localScale = new Vector3(0.04f, 0.04f, 1f);
            var branchSR = branchObj.AddComponent<SpriteRenderer>();
            branchSR.color = new Color(0.8f, 0.5f, 1f);
            branchObj.AddComponent<BoxCollider>();
            var branchRB = branchObj.AddComponent<Rigidbody>();
            branchRB.isKinematic = true;
            branchRB.useGravity = false;
        }

        // ── Canvas (WorldSpace) ──
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(root.transform, false);
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one;

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<GraphicRaycaster>();

        var canvasRT = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(1f, 1f);
        canvasRT.localScale = Vector3.one;

        // ── Header ──
        var headerObj = new GameObject("Header");
        headerObj.transform.SetParent(canvasObj.transform, false);

        var headerRT = headerObj.AddComponent<RectTransform>();
        headerRT.localScale = new Vector3(CONTAINER_SCALE, CONTAINER_SCALE, CONTAINER_SCALE);
        headerRT.anchoredPosition = new Vector2(0f, 0.02f);
        headerRT.sizeDelta = new Vector2(0.9f, 0.6f);

        var headerLayout = headerObj.AddComponent<VerticalLayoutGroup>();
        headerLayout.childForceExpandHeight = false;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childControlHeight = false;
        headerLayout.childControlWidth = true;
        headerLayout.spacing = -0.09f;
        headerLayout.childAlignment = TextAnchor.UpperCenter;

        var headerBG = headerObj.AddComponent<Image>();
        headerBG.color = new Color(0f, 0f, 0f, 0f);
        headerBG.raycastTarget = false;

        // idTmp
        var idObj = new GameObject("idTmp");
        idObj.transform.SetParent(headerObj.transform, false);
        var idRT = idObj.AddComponent<RectTransform>();
        idRT.sizeDelta = new Vector2(0f, HEADER_FONT + 0.04f);
        var idTmp = idObj.AddComponent<TextMeshProUGUI>();
        idTmp.text = "ID 1";
        idTmp.fontSize = HEADER_FONT;
        idTmp.alignment = TextAlignmentOptions.Center;
        idTmp.color = Color.white;
        idTmp.raycastTarget = false;
        idTmp.enableWordWrapping = false;
        idTmp.overflowMode = TextOverflowModes.Overflow;

        // statTmp
        var statObj = new GameObject("statTmp");
        statObj.transform.SetParent(headerObj.transform, false);
        var statRT = statObj.AddComponent<RectTransform>();
        statRT.sizeDelta = new Vector2(0f, HEADER_FONT + 0.04f);
        var statTmp = statObj.AddComponent<TextMeshProUGUI>();
        statTmp.text = "choice";
        statTmp.fontSize = HEADER_FONT * 0.8f;
        statTmp.alignment = TextAlignmentOptions.Center;
        statTmp.color = new Color(1f, 1f, 1f, 0.7f);
        statTmp.raycastTarget = false;
        statTmp.enableWordWrapping = false;
        statTmp.overflowMode = TextOverflowModes.Overflow;

        // ── NodeContent ──
        var content = CreateContentContainer(canvas.transform);

        // Count 라벨 (클릭 순환)
        CreateEnumLabelPreview(content, "lbl_count", "Count: 2",
            new Color(0.5f, 0.8f, 1f));

        // 선택지 4개 InputField
        for (int i = 0; i < 4; i++)
        {
            CreateInputFieldPreview(content, $"input_opt{i}",
                i < 2 ? $"선택지 {i + 1}" : "",
                $"선택지 {i + 1}", INPUT_HEIGHT, false);
        }

        return root;
    }

    // ══════════════════════════════════════════════════════
    // 공통: 노드 기본 구조 (Root + Prev + Next + Canvas + Header)
    // ══════════════════════════════════════════════════════
    private static GameObject CreateNodeBase(string name, Vector3 pos, Color spriteColor)
    {
        // ── Root ──
        var root = new GameObject(name);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var sr = root.AddComponent<SpriteRenderer>();
        sr.color = spriteColor;
        // 스프라이트는 기존 프리팹과 동일한 것을 사용 (여기서는 없으면 기본 흰색)

        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.25f, 0.52f, 0.01f);
        col.center = new Vector3(0f, -0.06f, 0f);

        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // ── Prev 버튼 ──
        var prevObj = new GameObject("Prev");
        prevObj.transform.SetParent(root.transform, false);
        prevObj.transform.localPosition = new Vector3(-0.05f, 0f, -0.21f);
        prevObj.transform.localScale = new Vector3(0.06757f, 0.06757f, 1f);
        var prevSR = prevObj.AddComponent<SpriteRenderer>();
        prevSR.color = new Color(0.3f, 0.8f, 0.3f);
        prevObj.AddComponent<BoxCollider>();
        var prevRB = prevObj.AddComponent<Rigidbody>();
        prevRB.isKinematic = true;
        prevRB.useGravity = false;

        // ── Next 버튼 ──
        var nextObj = new GameObject("Next");
        nextObj.transform.SetParent(root.transform, false);
        nextObj.transform.localPosition = new Vector3(0.05f, 0f, -0.21f);
        nextObj.transform.localScale = new Vector3(0.06757f, 0.06757f, 1f);
        var nextSR = nextObj.AddComponent<SpriteRenderer>();
        nextSR.color = new Color(0.8f, 0.3f, 0.3f);
        nextObj.AddComponent<BoxCollider>();
        var nextRB = nextObj.AddComponent<Rigidbody>();
        nextRB.isKinematic = true;
        nextRB.useGravity = false;

        // ── Canvas (WorldSpace) ──
        var canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(root.transform, false);
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one;

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<GraphicRaycaster>();

        var canvasRT = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(1f, 1f);
        canvasRT.localScale = Vector3.one;

        // ── Header (Image + VerticalLayout) ──
        var headerObj = new GameObject("Header");
        headerObj.transform.SetParent(canvasObj.transform, false);

        var headerRT = headerObj.AddComponent<RectTransform>();
        headerRT.localScale = new Vector3(CONTAINER_SCALE, CONTAINER_SCALE, CONTAINER_SCALE);
        headerRT.anchoredPosition = new Vector2(0f, 0.02f);
        headerRT.sizeDelta = new Vector2(0.9f, 0.6f);

        var headerLayout = headerObj.AddComponent<VerticalLayoutGroup>();
        headerLayout.childForceExpandHeight = false;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childControlHeight = false;
        headerLayout.childControlWidth = true;
        headerLayout.spacing = -0.09f;
        headerLayout.childAlignment = TextAnchor.UpperCenter;

        var headerBG = headerObj.AddComponent<Image>();
        headerBG.color = new Color(0f, 0f, 0f, 0f);
        headerBG.raycastTarget = false;

        // idTmp
        var idObj = new GameObject("idTmp");
        idObj.transform.SetParent(headerObj.transform, false);
        var idRT = idObj.AddComponent<RectTransform>();
        idRT.sizeDelta = new Vector2(0f, HEADER_FONT + 0.04f);
        var idTmp = idObj.AddComponent<TextMeshProUGUI>();
        idTmp.text = "ID 1";
        idTmp.fontSize = HEADER_FONT;
        idTmp.alignment = TextAlignmentOptions.Center;
        idTmp.color = Color.white;
        idTmp.raycastTarget = false;
        idTmp.enableWordWrapping = false;
        idTmp.overflowMode = TextOverflowModes.Overflow;

        // statTmp
        var statObj = new GameObject("statTmp");
        statObj.transform.SetParent(headerObj.transform, false);
        var statRT = statObj.AddComponent<RectTransform>();
        statRT.sizeDelta = new Vector2(0f, HEADER_FONT + 0.04f);
        var statTmp = statObj.AddComponent<TextMeshProUGUI>();
        statTmp.text = name.Replace("Ref_", "").ToLower();
        statTmp.fontSize = HEADER_FONT * 0.8f;
        statTmp.alignment = TextAlignmentOptions.Center;
        statTmp.color = new Color(1f, 1f, 1f, 0.7f);
        statTmp.raycastTarget = false;
        statTmp.enableWordWrapping = false;
        statTmp.overflowMode = TextOverflowModes.Overflow;

        return root;
    }

    // ══════════════════════════════════════════════════════
    // 공통: NodeContent 컨테이너 (데이터 영역)
    // ══════════════════════════════════════════════════════
    private static RectTransform CreateContentContainer(Transform canvasTransform)
    {
        var obj = new GameObject("NodeContent");
        obj.transform.SetParent(canvasTransform, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.localScale = new Vector3(CONTAINER_SCALE, CONTAINER_SCALE, CONTAINER_SCALE);
        rt.anchoredPosition = new Vector2(0f, -0.055f);
        rt.sizeDelta = new Vector2(0.9f, 2f);

        var layout = obj.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.spacing = LAYOUT_SPACING;
        layout.childAlignment = TextAnchor.UpperCenter;

        return rt;
    }

    // ══════════════════════════════════════════════════════
    // 공통: TMP_InputField 미리보기 생성
    // ══════════════════════════════════════════════════════
    private static TMP_InputField CreateInputFieldPreview(RectTransform parent, string name,
        string text, string placeholder, float height, bool multiline)
    {
        // Root
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(0f, height);

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        // Text Area
        var textAreaObj = new GameObject("Text Area");
        textAreaObj.transform.SetParent(root.transform, false);
        var textAreaRT = textAreaObj.AddComponent<RectTransform>();
        textAreaRT.anchorMin = Vector2.zero;
        textAreaRT.anchorMax = Vector2.one;
        textAreaRT.offsetMin = new Vector2(0.02f, 0.01f);
        textAreaRT.offsetMax = new Vector2(-0.02f, -0.01f);
        textAreaObj.AddComponent<RectMask2D>();

        // Text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(textAreaObj.transform, false);
        var textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = INPUT_FONT;
        tmp.color = Color.white;
        tmp.enableWordWrapping = multiline;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.richText = false;

        // Placeholder
        var phObj = new GameObject("Placeholder");
        phObj.transform.SetParent(textAreaObj.transform, false);
        var phRT = phObj.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero;
        phRT.offsetMax = Vector2.zero;
        var phTmp = phObj.AddComponent<TextMeshProUGUI>();
        phTmp.fontSize = INPUT_FONT;
        phTmp.color = new Color(1f, 1f, 1f, 0.3f);
        phTmp.text = placeholder;
        phTmp.enableWordWrapping = multiline;
        phTmp.fontStyle = FontStyles.Italic;

        // TMP_InputField 컴포넌트
        var input = root.AddComponent<TMP_InputField>();
        input.textViewport = textAreaRT;
        input.textComponent = tmp;
        input.placeholder = phTmp;
        input.text = text;
        input.pointSize = INPUT_FONT;
        input.caretWidth = 2;

        if (multiline)
        {
            input.lineType = TMP_InputField.LineType.MultiLineNewline;
            input.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            input.contentType = TMP_InputField.ContentType.Standard;
        }

        return input;
    }

    // ══════════════════════════════════════════════════════
    // 공통: Enum 클릭 순환 라벨 미리보기
    // ══════════════════════════════════════════════════════
    private static TextMeshProUGUI CreateEnumLabelPreview(RectTransform parent, string name,
        string previewText, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, LABEL_HEIGHT);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = LABEL_FONT;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.text = previewText;
        tmp.raycastTarget = true;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        // EventTrigger (클릭 이벤트 - 런타임에서 코드로 연결)
        obj.AddComponent<EventTrigger>();

        return tmp;
    }
}
#endif
