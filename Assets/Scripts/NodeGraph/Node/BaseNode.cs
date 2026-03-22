using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// ���⼭ ������ �ϳ� �߸���
/// ���� �ܼ��� �׷����� �����ϱ� ���� ������ ���Ǿ�����ߴ�.
/// �׸��� ���־�뺧�� �����͸� ������ �ɹ� ������ ���۽�Ű�� 
/// ���� ������ ���־�뺧 ������ �Ϻ��ϰ� �и��ȴ�.
/// </summary>


public abstract class BaseNode : MonoBehaviour , INodeDataGetService
{
    [Header("Prefab")]
    public SpriteRenderer bg_sprite;
    public GameObject edge;
    public TMP_FontAsset nodeFont;

    [Header("Size")]
    [Tooltip("노드의 월드 크기 (X=가로, Y=세로). 이 값 하나로 Canvas·버튼·콜라이더가 자동 맞춰짐.")]
    public Vector2 nodeSize = new Vector2(1.4f, 1.0f);

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
    public Action<BaseNode, Vector3> Dragging;
    public event Action<BaseNode> Dropped;
    protected Vector3 _dragOffset;
    public float offsetY = 0f;
    public float snap;

    public TsvCommand tsvCommand;

    public void Awake()
    {
        if (!cam) cam = Camera.main;
        if (prevButton != null)
        {
            prevButton.MyNode = this;
            prevButton.press += OnPressPrev;
        }
        if (nextButton != null)
        {
            nextButton.MyNode = this;
            nextButton.press += OnPressNext;
            nextButton.endPress += OnNextEndPress;
        }

        // nodeSize 기준으로 Canvas·버튼·콜라이더 동기화
        ApplyNodeSize();
    }

    /// <summary>
    /// 노드 인스턴스화 직후 호출. BG bounds가 확정된 시점에 콜라이더·버튼 위치를 정확히 맞춘다.
    /// </summary>
    public void Setup()
    {
        if (bg_sprite == null) return;

        var bounds = bg_sprite.bounds;

        // ── BoxCollider: XY 평면 스프라이트 기준 (Z = 얇은 깊이) ──
        var col = GetComponent<BoxCollider>();
        if (col != null)
            col.size = new Vector3(bounds.size.x, bounds.size.y, 0.1f);

        // ── 버튼: BG 실제 좌우 끝 ──────────────────────────
        Vector3 leftLocal  = transform.InverseTransformPoint(
            new Vector3(bounds.min.x, bounds.center.y, bounds.center.z));
        Vector3 rightLocal = transform.InverseTransformPoint(
            new Vector3(bounds.max.x, bounds.center.y, bounds.center.z));

        if (prevButton != null) prevButton.transform.localPosition = leftLocal;
        if (nextButton != null) nextButton.transform.localPosition = rightLocal;
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
        // LinkButton 위를 클릭했으면 노드 드래그 차단
        if (IsPointerOverLinkButton()) return;

        NodeGraphManager.instance.selectNode = this;

        // 에디터 패널 열기
        NodeEditorPanel.instance?.Open(this);

        dragging = true;
        BeginDrag();
        Picked?.Invoke(this);
    }

    private bool IsPointerOverLinkButton()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray);
        foreach (var hit in hits)
            if (hit.collider.GetComponent<LinkButton>() != null)
                return true;
        return false;
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

    private void Update()
    {
        if (!dragging) return;

        // Screen Space Overlay UI가 OnMouseUp을 가로챌 때를 대비한 백업
        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
            return;
        }

        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray   = cam.ScreenPointToRay(inputPos);
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
            mousePos = ray.GetPoint(enter) + _dragOffset;
        else
            mousePos = transform.position;

        Dragging?.Invoke(this, transform.position);
        SetPosition(mousePos);

        if (nextEdge != null && NextNode != null)
            nextEdge.SetNameless(nextButton.transform.position, NextNode.prevButton.transform.position);

        UpdateAllIncomingEdges();
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
        if (snap <= 0f)
        {
            UpdateAllIncomingEdges();
            return;
        }

        Vector2Int gridPos = WorldToRectGrid(transform.position, snap);
        Vector3 snapPos = RectGridToWorld(gridPos, snap);

        SetPosition(snapPos);

        if (nextEdge != null && NextNode != null)
            nextEdge.SetNameless(nextButton.transform.position, NextNode.prevButton.transform.position);

        UpdateAllIncomingEdges();
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

    public virtual Vector3 GetEdgeStartPosition(BaseNode target)
    {
        return nextButton.transform.position;
    }

    protected void UpdateAllIncomingEdges()
    {
        var nodes = NodeGraphManager.instance.nodes;
        for (int n = 0; n < nodes.Count; n++)
        {
            var node = nodes[n];
            if (node == this) continue;

            if (node.nextEdge != null && node.NextNode == this)
                node.nextEdge.SetNameless(node.nextButton.transform.position, prevButton.transform.position);

            var choice = node as Choice;
            if (choice != null)
            {
                for (int i = 0; i < choice.branchNodes.Count; i++)
                {
                    if (choice.branchNodes[i] == this
                        && i < choice.branchEdges.Count && choice.branchEdges[i] != null
                        && i < choice.branchButtons.Count && choice.branchButtons[i] != null)
                    {
                        choice.branchEdges[i].SetNameless(
                            choice.branchButtons[i].transform.position,
                            prevButton.transform.position);
                    }
                }
            }
        }
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

        if (nodeFont != null)
        {
            if (idTmp != null) idTmp.font = nodeFont;
            if (statTmp != null) statTmp.font = nodeFont;
        }
        if (idTmp != null)   idTmp.text   = $"ID {cmd.Id}";
        else Debug.LogWarning($"[BaseNode] idTmp가 null입니다 — 프리팹 '{gameObject.name}'에서 AutoSetup을 실행하세요.");
        if (statTmp != null) statTmp.text = cmd.StateStr;
        else Debug.LogWarning($"[BaseNode] statTmp가 null입니다 — 프리팹 '{gameObject.name}'에서 AutoSetup을 실행하세요.");

        gameObject.name = $"node {cmd.Id}";
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

    // ── Size Apply ───────────────────────────────────────

    /// <summary>
    /// nodeSize(월드 단위) 기준으로 Canvas·Background·버튼·콜라이더를 한 번에 맞춘다.
    /// Inspector에서 nodeSize만 바꾸면 나머지가 자동으로 따라온다.
    /// </summary>
    public void ApplyBgColor(Color color)
    {
        if (bg_sprite != null) bg_sprite.color = color;
    }

    public void ApplyNodeSize()
    {
        float w = nodeSize.x;
        float h = nodeSize.y;

        // ── BG 스프라이트 스케일 ─────────────────────────
        if (bg_sprite != null && bg_sprite.sprite != null)
        {
            // 스프라이트의 픽셀→월드 단위 네이티브 크기
            var rect = bg_sprite.sprite.rect;
            float ppu  = bg_sprite.sprite.pixelsPerUnit;
            float nativeW = rect.width  / ppu;
            float nativeH = rect.height / ppu;

            bg_sprite.transform.localScale = new Vector3(
                nativeW > 0f ? w / nativeW : 1f,
                nativeH > 0f ? h / nativeH : 1f,
                1f
            );
        }

        // ── Canvas sizeDelta + 마스크 ────────────────────
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            float s = canvas.transform.localScale.x;
            if (s > 0f)
                canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(w / s, h / s);

        }

        // ── Prev / Next 버튼: BG 중심에서 X축 ±w/2 ────
        if (bg_sprite != null)
        {
            Vector3 bgPos = bg_sprite.transform.localPosition;
            if (prevButton != null)
                prevButton.transform.localPosition = new Vector3(bgPos.x - w * 0.5f, bgPos.y, bgPos.z);
            if (nextButton != null)
                nextButton.transform.localPosition = new Vector3(bgPos.x + w * 0.5f, bgPos.y, bgPos.z);
        }

        // ── BoxCollider: XY 평면 스프라이트 기준 ────────
        var col = GetComponent<BoxCollider>();
        if (col != null)
            col.size = new Vector3(w, h, 0.1f);
    }

    // ── Auto Setup ───────────────────────────────────────

    /// <summary>
    /// 컴포넌트를 처음 붙일 때 자동 호출. 직접 실행하려면 우클릭 → Auto Setup.
    /// </summary>
    [ContextMenu("Auto Setup")]
    public void AutoSetup()
    {
        // BG 스프라이트: "BG" 이름 오브젝트 우선, 없으면 첫 번째 SpriteRenderer, 없으면 자동 생성
        var bgTr = transform.Find("BG");
        if (bgTr != null)
            bg_sprite = bgTr.GetComponent<SpriteRenderer>();
        if (bg_sprite == null)
            bg_sprite = GetComponentInChildren<SpriteRenderer>(true);
        if (bg_sprite == null)
        {
            var bgGo = new GameObject("BG");
            bgGo.transform.SetParent(transform, false);
            bgGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            bg_sprite = bgGo.AddComponent<SpriteRenderer>();
            Debug.Log($"[AutoSetup] '{gameObject.name}' — BG 오브젝트 자동 생성. Inspector에서 스프라이트를 할당하세요.");
        }

        // LinkButton: isPrev 플래그로 구분
        foreach (var btn in GetComponentsInChildren<LinkButton>(true))
        {
            if (btn.isPrev) prevButton = btn;
            else            nextButton = btn;
        }

        // TMP 라벨: Canvas 직계 자식 순서로 구분 (0번=idTmp, 1번=statTmp)
        var canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            var tmpsInCanvas = new System.Collections.Generic.List<TextMeshProUGUI>();
            foreach (Transform child in canvas.transform)
            {
                var tmp = child.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmpsInCanvas.Add(tmp);
            }
            if (tmpsInCanvas.Count > 0) idTmp   = tmpsInCanvas[0];
            if (tmpsInCanvas.Count > 1) statTmp = tmpsInCanvas[1];
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // 컴포넌트를 Inspector에서 처음 붙일 때 Unity가 자동 호출
    private void Reset() => AutoSetup();

}

