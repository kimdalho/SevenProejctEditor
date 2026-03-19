using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ���⼭ ������ �ϳ� �߸���
/// ���� �ܼ��� �׷����� �����ϱ� ���� ������ ���Ǿ�����ߴ�.
/// �׸��� ���־�뺧�� �����͸� ������ �ɹ� ������ ���۽�Ű�� 
/// ���� ������ ���־�뺧 ������ �Ϻ��ϰ� �и��ȴ�.
/// </summary>


public class BaseNode : MonoBehaviour , INodeDataGetService
{
    [Header("Prefab")]
    public GameObject edge;

    [Header("Data")]
    // ===== NodeData =====
    public int groupId;
    public int id;    
    public string character;
    public string str_1;
    public float wait;

    public Vector3 DataPos;
    protected Camera cam;

    [Header("Runtime")]
    // ===== Runtime =====
    [SerializeField] protected bool dragging = false;
    [SerializeField] protected Vector3 mousePos;
    public LinkButton prevButton;
    public LinkButton nextButton;

    public Edge prevEdge;
    public Edge nextEdge;

    public BaseNode NextNode;
    public BaseNode PrevNode;   

    public TextMeshProUGUI idTmp;
    public TextMeshProUGUI statTmp;

    public event Action<BaseNode> Picked;
    public event Action<BaseNode, Vector3> Dragging;
    public event Action<BaseNode> Dropped;
    private Vector3 _dragOffset;
    public float offsetY = 0f;
    public float snap;

    public TsvCommand tsvCommand;

    protected RectTransform contentContainer;
    protected bool nodeUIBuilt = false;

    public void Awake()
    {
        if (!cam) cam = Camera.main;
        if (prevButton == null || nextButton == null) return;
        prevButton.MyNode = this;
        nextButton.MyNode = this;
        prevButton.press += OnPressPrev;
        nextButton.press += OnPressNext;
        nextButton.endPress += OnNextEndPress;
    }

    private void OnNextEndPress()
    {
        var nextNode = nextEdge.GetNextNode();
        if(nextNode != null)
        {
            //Old
            if (NextNode != null && NextNode != nextNode) 
            {
                NextNode.PrevNode = null;
                NextNode.prevEdge = null;
            }


            NextNode = nextNode;
        }
        else
        {
            //Old
            if (NextNode != null)
            {
                NextNode.PrevNode = null;
                NextNode.prevEdge = null;
            }
        }
    }

    private void OnPressPrev(Vector3 vector)
    {
        
    }

    private void OnPressNext(Vector3 vector)
    {
        if(nextEdge == null)
        {
            var obj =   Instantiate(edge);
            nextEdge = obj.GetComponent<Edge>();
            nextEdge.MyNode = this;
            nextEdge.ban.Add(prevButton);
            nextEdge.ban.Add(nextButton);
        }

        nextEdge.SetDrawLine(nextButton.gameObject.transform.position,vector);
    }

    public void OnMouseDown()
    {
        // InputField 클릭 시 드래그 방지
        if (IsPointerOverInputField())
            return;

        NodeGraphManager.instance.selectNode = this;
        dragging = true;
        BeginDrag();
        Picked?.Invoke(this);
    }

    void BeginDrag()
    {
        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray = cam.ScreenPointToRay(inputPos);

        // ������Ʈ�� ���� ���̸� ������ ���
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
        {
            var hit = ray.GetPoint(enter);
            _dragOffset = transform.position - hit;   // �߽�-Ŭ������ ���� ����
        }
        else
        {         
            _dragOffset = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (!dragging) return;

        // 1) ������ �� ��� ��Ʈ
        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray = cam.ScreenPointToRay(inputPos);
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
        {
            var hit = ray.GetPoint(enter);
            mousePos = hit + _dragOffset;  // ������ ����
            
        }
        else
        {
            mousePos = transform.position;
            
        }      

        Dragging?.Invoke(this, transform.position);
        SetPosition(mousePos);

        if(nextEdge != null && NextNode != null)
            nextEdge.SetNameless(nextButton.transform.position, NextNode.prevButton.transform.position);

        if (prevEdge != null && PrevNode != null)
            PrevNode.nextEdge.SetNameless(PrevNode.nextButton.transform.position, PrevNode.NextNode.prevButton.transform.position);
    }

    public void OnMouseUp()
    {
        if (!dragging) return;

        var snapped = transform.position;
        snapped.y = offsetY;
        transform.position = snapped;

        dragging = false;
        Dropped?.Invoke(this);
        OnSnapshot();
    }

    public void OnSnapshot()
    {
        Vector2Int gridPos = WorldToRectGrid(transform.position, snap);
        Vector3 snapPos = RectGridToWorld(gridPos, snap);

        SetPosition(snapPos);

        if (nextEdge != null && NextNode != null)
            nextEdge.SetNameless(nextButton.transform.position, NextNode.prevButton.transform.position);

        if (prevEdge != null && PrevNode != null)
            PrevNode.nextEdge.SetNameless(PrevNode.nextButton.transform.position, PrevNode.NextNode.prevButton.transform.position);
    }
    protected Vector2Int WorldToRectGrid(Vector3 worldPos, float cellSize)
    {
        int gx = Mathf.RoundToInt(worldPos.x / cellSize);
        int gy = Mathf.RoundToInt(worldPos.z / cellSize);
        return new Vector2Int(gx, gy);
    }

    protected Vector3 RectGridToWorld(Vector2Int gridPos, float cellSize)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    public void SetPosition(Vector3 pos)
    {
        pos.y = offsetY;
        transform.position = pos;
        DataPos = pos;
    }


    public virtual void SetCommnadData(TsvCommand cmd)
    {
        tsvCommand = cmd;
        character = cmd.Get("character");
        str_1 = cmd.Get("str_1");
        id = cmd.Id;

        // wait / duration 읽기
        if (cmd.Has("wait"))
            wait = cmd.GetFloat("wait", 0f);
        else if (cmd.Has("duration"))
            wait = cmd.GetFloat("duration", 0f);

        idTmp.text = $"ID {cmd.Id}";
        statTmp.text = cmd.StateStr;

        gameObject.name = $"node {cmd.Id}";

        if (!nodeUIBuilt) { BuildNodeUI(); nodeUIBuilt = true; }
        RefreshNodeUI();
    }

    public void SetLinkNext(BaseNode baseNode)
    {
        //1) ��� ����
        if (nextEdge == null)
        {
            var obj = Instantiate(edge);
            nextEdge = obj.GetComponent<Edge>();
            nextEdge.MyNode = this;
            nextEdge.ban.Add(prevButton);
            nextEdge.ban.Add(nextButton);
            nextEdge.gameObject.SetActive(true);
        }
        
        
        //2) ��� ����
        NextNode = baseNode;        
        baseNode.PrevNode = this;
        baseNode.prevEdge = nextEdge;

        nextEdge.SetNameless(nextButton.transform.position, NextNode.prevButton.transform.position);
    }

    public void Remove()
    {
        if(PrevNode != null)
        {
            PrevNode.NextNode = null;
            prevEdge.Remove();
        }
           

        if (NextNode != null)
        {
            NextNode.PrevNode = null;
            nextEdge.Remove();
        }
                            
        NodeGraphManager.instance.nodes.Remove(this);        
        Destroy(this.gameObject);
    }

    public virtual IEnumerator Execute(NodePlayer player)
    {
        yield break;
    }

    public void SetData()
    {

    }

    // ── virtual UI 빌드/갱신 ─────────────────────────────

    protected virtual void BuildNodeUI() { }
    protected virtual void RefreshNodeUI() { }

    // ── 드래그 가드 ──────────────────────────────────────

    protected bool IsPointerOverInputField()
    {
        if (EventSystem.current == null) return false;
        var pointer = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);
        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<TMP_InputField>() != null ||
                r.gameObject.GetComponentInParent<TMP_InputField>() != null)
                return true;
        }
        return false;
    }

    // ── UI 헬퍼: 콘텐츠 컨테이너 ────────────────────────

    protected RectTransform GetOrCreateContentContainer()
    {
        if (contentContainer != null) return contentContainer;

        var canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) return null;

        var existing = canvas.transform.Find("NodeContent");
        if (existing != null)
        {
            contentContainer = existing.GetComponent<RectTransform>();
            return contentContainer;
        }

        var obj = new GameObject("NodeContent");
        obj.transform.SetParent(canvas.transform, false);

        contentContainer = obj.AddComponent<RectTransform>();
        contentContainer.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        contentContainer.anchoredPosition = new Vector2(0f, -0.055f);
        contentContainer.sizeDelta = new Vector2(0.9f, 2f);

        var layout = obj.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.spacing = -0.02f;
        layout.childAlignment = TextAnchor.UpperCenter;

        return contentContainer;
    }

    // ── UI 헬퍼: TMP_InputField 생성 ─────────────────────

    protected TMP_InputField CreateTMPInputField(Transform parent, string name,
        string text, string placeholder, float height, float fontSize,
        TMP_InputField.ContentType contentType, bool multiline,
        Action<string> onChanged)
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
        tmp.fontSize = fontSize;
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
        phTmp.fontSize = fontSize;
        phTmp.color = new Color(1f, 1f, 1f, 0.3f);
        phTmp.text = placeholder ?? "";
        phTmp.enableWordWrapping = multiline;
        phTmp.fontStyle = FontStyles.Italic;

        // InputField
        var input = root.AddComponent<TMP_InputField>();
        input.textViewport = textAreaRT;
        input.textComponent = tmp;
        input.placeholder = phTmp;
        input.text = text ?? "";
        input.pointSize = fontSize;
        input.contentType = contentType;
        input.caretWidth = 2;

        if (multiline)
            input.lineType = TMP_InputField.LineType.MultiLineNewline;

        if (onChanged != null)
            input.onValueChanged.AddListener((val) => onChanged(val));

        return input;
    }

    // ── UI 헬퍼: Enum 셀렉터 (클릭 순환 라벨) ───────────

    protected TextMeshProUGUI CreateEnumSelector(Transform parent, string name,
        Color color, float fontSize, Action onClick)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, fontSize + 0.04f);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = true;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        if (onClick != null)
        {
            var trigger = obj.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((_) => onClick());
            trigger.triggers.Add(entry);
        }

        return tmp;
    }

    // ── UI 헬퍼: 읽기 전용 라벨 ─────────────────────────

    protected TextMeshProUGUI CreateLabel(Transform parent, string name,
        Color color, float fontSize)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, fontSize + 0.04f);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        return tmp;
    }
}
