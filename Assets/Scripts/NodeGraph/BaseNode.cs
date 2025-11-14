using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BaseNode : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject edge;

    [Header("Data")]
    // ===== NodeData =====
    public int groupId;
    public int id;
    public Vector3 DataPos;
    protected Camera cam;
    public string character;
    public string str_1;
    

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

    public event Action<BaseNode> Picked;
    public event Action<BaseNode, Vector3> Dragging;
    public event Action<BaseNode> Dropped;
    private Vector3 _dragOffset;
    public float offsetY = 0f;
    public float snap;

    public TsvCommand tsvCommand;



    public void Awake()
    {
        if (!cam) cam = Camera.main;
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
        dragging = true;
        BeginDrag();
        Picked?.Invoke(this);
    }

    void BeginDrag()
    {
        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray = cam.ScreenPointToRay(inputPos);

        // 오브젝트의 현재 높이를 지나는 평면
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
        {
            var hit = ray.GetPoint(enter);
            _dragOffset = transform.position - hit;   // 중심-클릭지점 차이 보존
        }
        else
        {         
            _dragOffset = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (!dragging) return;

        // 1) 포인터 → 평면 히트
        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray = cam.ScreenPointToRay(inputPos);
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
        {
            var hit = ray.GetPoint(enter);
            mousePos = hit + _dragOffset;  // 오프셋 적용
            
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


    public void SetCommnadData(TsvCommand cmd)
    {
        character = cmd.Get("character");
        str_1 = cmd.Get("str_1");
        id = cmd.Id;    
    }

    public void SetLinkNext(BaseNode baseNode)
    {
        //1) 노드 생성
        if (nextEdge == null)
        {
            var obj = Instantiate(edge);
            nextEdge = obj.GetComponent<Edge>();
            nextEdge.MyNode = this;
            nextEdge.ban.Add(prevButton);
            nextEdge.ban.Add(nextButton);
            nextEdge.gameObject.SetActive(true);
        }
        
        
        //2) 노드 연결
        NextNode = baseNode;        
        baseNode.PrevNode = this;
        baseNode.prevEdge = nextEdge;

        nextEdge.SetNameless(nextButton.transform.position, NextNode.prevButton.transform.position);
    }
}
