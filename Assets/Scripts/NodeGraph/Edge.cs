using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public BaseNode MyNode;
    public BaseNode prevNode;
    public BaseNode nextNode;

    public GameObject prefab;

    public GameObject targetDot;
    public List<LinkButton> ban = new();
    public BaseNode data;

    [Header("Curve Settings")]
    public Color lineColor = Color.white;
    public float lineWidth = 0.03f;
    public int curveSegments = 24;

    private LineRenderer lr;
    private bool oldRendererHidden;

    // ── LineRenderer 초기화 ──────────────────────────────
    private void EnsureLineRenderer()
    {
        if (lr != null) return;

        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = curveSegments + 1;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        // Sprites/Default 셰이더 (Unlit, 알파 지원)
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = lineColor;
        lr.material = mat;
        lr.sortingOrder = -1;

        // 기존 직선 렌더러 숨기기
        HideOldRenderer();
    }

    private void HideOldRenderer()
    {
        if (oldRendererHidden) return;
        oldRendererHidden = true;

        // 자기 자신 + 모든 자식의 렌더러 끄기 (LineRenderer 제외)
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            if (r is LineRenderer) continue;
            r.enabled = false;
        }
    }

    // ── 큐빅 베지어 ─────────────────────────────────────
    private static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0
             + 3f * u * u * t * p1
             + 3f * u * t * t * p2
             + t * t * t * p3;
    }

    /// <summary>
    /// start → end 사이에 베지어 곡선을 그린다.
    /// 핸들 방향: start에서 +X로 나가고, end에서 -X로 들어옴 (노드 그래프 표준)
    /// </summary>
    private void DrawCurve(Vector3 start, Vector3 end)
    {
        EnsureLineRenderer();

        // 노드와 같은 Y 평면에 그리기 (카메라 시야에 확실히 보이도록)
        float edgeY = start.y;
        end.y = edgeY;

        // 핸들 길이: X거리의 절반, 최소 0.15 보장
        float dx = Mathf.Abs(end.x - start.x);
        float dz = Mathf.Abs(end.z - start.z);
        float handleLen = Mathf.Max(dx * 0.5f, dz * 0.3f, 0.15f);

        // 노드가 역방향(end.x < start.x)이면 핸들을 더 크게
        if (end.x < start.x)
            handleLen = Mathf.Max(handleLen, dz * 0.5f + 0.3f);

        Vector3 cp1 = start + Vector3.right * handleLen;
        Vector3 cp2 = end - Vector3.right * handleLen;

        lr.positionCount = curveSegments + 1;
        for (int i = 0; i <= curveSegments; i++)
        {
            float t = i / (float)curveSegments;
            lr.SetPosition(i, CubicBezier(start, cp1, cp2, end, t));
        }

        // 기존 transform 조작 무력화
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        transform.position = start;
    }

    // ── 기존 API (곡선으로 교체) ─────────────────────────

    public void SetNameless(Vector3 start, Vector3 end)
    {
        if (targetDot == null)
            targetDot = Instantiate(prefab);

        DrawCurve(start, end);
        targetDot.transform.position = end;
    }

    public void SetDrawLine(Vector3 start, Vector3 end)
    {
        if (targetDot == null)
            targetDot = Instantiate(prefab);

        gameObject.SetActive(true);
        targetDot.gameObject.SetActive(true);

        // 스냅 대상 탐색
        var nearest = FindNearestDotExcludingSelf(end);

        if (nearest && Vector3.Distance(end, nearest.transform.position) <= 0.5f)
        {
            targetDot.transform.position = nearest.transform.position;
            DrawCurve(start, targetDot.transform.position);

            nextNode = nearest.MyNode;
            nextNode.PrevNode = MyNode;
            nextNode.prevEdge = this;
        }
        else
        {
            if (nextNode != null)
            {
                nextNode.PrevNode = null;
                nextNode.prevEdge = null;
                nextNode = null;
            }

            targetDot.transform.position = end;
            DrawCurve(start, end);
        }
    }

    public BaseNode GetNextNode()
    {
        if (nextNode == null)
        {
            gameObject.SetActive(false);
            targetDot.SetActive(false);
        }
        return nextNode;
    }

    private LinkButton FindNearestDotExcludingSelf(Vector3 from)
    {
        LinkButton best = null;
        float bestDist = float.MaxValue;
        var all = FindObjectsOfType<LinkButton>();
        for (int i = 0; i < all.Length; i++)
        {
            var d = all[i];
            if (d == this) continue;
            if (d.isPrev is false) continue;
            if (ban.Contains(d)) continue;
            float dist = Vector3.Distance(from, d.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = d;
            }
        }
        return best;
    }

    public void Remove()
    {
        if (targetDot != null)
            Destroy(targetDot.gameObject);

        Destroy(gameObject);
    }
}
