using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Choice : BaseNode
{
    [Header("Branch")]
    public List<string> options = new();
    public List<LinkButton> branchButtons = new();
    public List<Edge> branchEdges = new();
    public List<BaseNode> branchNodes = new();
    public List<int> targetIds = new();

    private int selectedIndex = -1;

    // 에디터 UI
    private TextMeshProUGUI countLabel;
    private TMP_InputField[] optionInputs = new TMP_InputField[4];

    public new void Awake()
    {
        if (!cam) cam = Camera.main;
        if (prevButton != null)
        {
            prevButton.MyNode = this;
            prevButton.press += OnPressPrev;
        }
        // nextButton은 사용하지 않음 (branchButton 사용)

        for (int i = 0; i < branchButtons.Count; i++)
        {
            int idx = i;
            var btn = branchButtons[i];
            if (btn == null) continue;
            btn.MyNode = this;
            btn.press += (pos) => OnPressBranch(idx, pos);
            btn.endPress += () => OnEndPressBranch(idx);
        }
    }

    private void OnPressPrev(Vector3 vector) { }

    // ── 엣지 관리 ──────────────────────────────────────

    private void OnPressBranch(int idx, Vector3 pos)
    {
        while (branchEdges.Count <= idx) branchEdges.Add(null);
        while (branchNodes.Count <= idx) branchNodes.Add(null);

        if (branchEdges[idx] == null)
        {
            var obj = Instantiate(edge);
            branchEdges[idx] = obj.GetComponent<Edge>();
            branchEdges[idx].MyNode = this;
            branchEdges[idx].ban.Add(prevButton);
            foreach (var btn in branchButtons)
                if (btn != null) branchEdges[idx].ban.Add(btn);
        }

        branchEdges[idx].SetDrawLine(branchButtons[idx].transform.position, pos);
    }

    private void OnEndPressBranch(int idx)
    {
        if (idx >= branchEdges.Count || branchEdges[idx] == null) return;
        var target = branchEdges[idx].GetNextNode();
        while (branchNodes.Count <= idx) branchNodes.Add(null);

        if (target != null)
        {
            // 기존 연결 해제
            if (branchNodes[idx] != null && branchNodes[idx] != target)
            {
                branchNodes[idx].PrevNode = null;
                branchNodes[idx].prevEdge = null;
            }
            branchNodes[idx] = target;
        }
        else
        {
            if (branchNodes[idx] != null)
            {
                branchNodes[idx].PrevNode = null;
                branchNodes[idx].prevEdge = null;
            }
            branchNodes[idx] = null;
        }
    }

    // ── 드래그 중 엣지 업데이트 ────────────────────────

    private void FixedUpdate()
    {
        if (!dragging) return;

        var inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        var ray = cam.ScreenPointToRay(inputPos);
        var plane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
        if (plane.Raycast(ray, out var enter))
        {
            var hit = ray.GetPoint(enter);
            mousePos = hit + _dragOffset;
        }
        else
        {
            mousePos = transform.position;
        }

        Dragging?.Invoke(this, transform.position);
        SetPosition(mousePos);

        // 브랜치 엣지 업데이트
        for (int i = 0; i < branchEdges.Count; i++)
        {
            if (branchEdges[i] != null && i < branchNodes.Count && branchNodes[i] != null
                && i < branchButtons.Count && branchButtons[i] != null)
            {
                branchEdges[i].SetNameless(
                    branchButtons[i].transform.position,
                    branchNodes[i].prevButton.transform.position);
            }
        }

        UpdateAllIncomingEdges();
    }

    // ── TSV 파싱 ────────────────────────────────────────

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);

        string optStr = cmd.Get("options");
        if (!string.IsNullOrEmpty(optStr))
            options = new List<string>(optStr.Split('|'));

        string targetStr = cmd.Get("targets");
        if (!string.IsNullOrEmpty(targetStr))
        {
            targetIds.Clear();
            foreach (var s in targetStr.Split('|'))
                if (int.TryParse(s.Trim(), out int tid))
                    targetIds.Add(tid);
        }

        // 최소 2개 선택지 보장
        while (options.Count < 2) options.Add("");

        RefreshNodeUI();
    }

    // ── 에디터 UI 생성 ─────────────────────────────────

    protected override void BuildNodeUI()
    {
        var container = GetOrCreateContentContainer();
        if (container == null) return;

        // 선택지 개수 라벨 (클릭 → 2/3/4 순환)
        countLabel = CreateEnumSelector(container, "lbl_count",
            new Color(0.5f, 0.8f, 1f), 0.22f,
            () => { CycleOptionCount(); RefreshNodeUI(); SyncToTsvCommand(); });

        // 선택지 4개 InputField
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            optionInputs[i] = CreateTMPInputField(container, $"input_opt{i}",
                i < options.Count ? options[i] : "", $"선택지 {i + 1}",
                0.3f, 0.2f, TMP_InputField.ContentType.Standard, false,
                (val) => { SetOption(idx, val); SyncToTsvCommand(); });
        }
    }

    protected override void RefreshNodeUI()
    {
        if (countLabel == null) return;

        countLabel.text = $"Count: {options.Count}";

        for (int i = 0; i < 4; i++)
        {
            if (optionInputs[i] == null) continue;

            if (i < options.Count)
            {
                optionInputs[i].gameObject.SetActive(true);
                optionInputs[i].SetTextWithoutNotify(options[i]);

                // 대응하는 branchButton 활성화
                if (i < branchButtons.Count && branchButtons[i] != null)
                    branchButtons[i].gameObject.SetActive(true);
            }
            else
            {
                optionInputs[i].gameObject.SetActive(false);

                // 대응하는 branchButton 비활성화
                if (i < branchButtons.Count && branchButtons[i] != null)
                    branchButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void CycleOptionCount()
    {
        int count = options.Count;
        count++;
        if (count > 4) count = 2;

        while (options.Count < count) options.Add("");
        while (options.Count > count) options.RemoveAt(options.Count - 1);
    }

    private void SetOption(int idx, string val)
    {
        while (options.Count <= idx) options.Add("");
        options[idx] = val;
    }

    // ── TSV 동기화 ──────────────────────────────────────

    private void SyncToTsvCommand()
    {
        if (tsvCommand == null) return;
        if (tsvCommand.Fields == null)
            tsvCommand.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        tsvCommand.Fields["options"] = string.Join("|", options);

        var ids = new List<string>();
        for (int i = 0; i < options.Count; i++)
        {
            if (i < branchNodes.Count && branchNodes[i] != null)
                ids.Add(branchNodes[i].id.ToString());
            else
                ids.Add("-1");
        }
        tsvCommand.Fields["targets"] = string.Join("|", ids);
        tsvCommand.StateInt = options.Count;
    }

    // ── 런타임 실행 ─────────────────────────────────────

    public override IEnumerator Execute(NodePlayer player)
    {
        var choiceUI = player.choiceUI;
        yield return choiceUI.ShowChoices(options);
        selectedIndex = choiceUI.SelectedIndex;

        // 선택된 분기의 목표 노드로 NextNode 교체
        if (selectedIndex >= 0 && selectedIndex < branchNodes.Count
            && branchNodes[selectedIndex] != null)
        {
            NextNode = branchNodes[selectedIndex];
        }
        else
        {
            NextNode = null;
        }
    }

    public override Vector3 GetEdgeStartPosition(BaseNode target)
    {
        int idx = branchNodes.IndexOf(target);
        if (idx >= 0 && idx < branchButtons.Count && branchButtons[idx] != null)
            return branchButtons[idx].transform.position;
        return transform.position;
    }

    // ── TSV 로드 후 분기 타겟 복원 ──────────────────────

    public void ResolveBranchTargets(List<BaseNode> allNodes)
    {
        branchNodes.Clear();
        foreach (int tid in targetIds)
        {
            var target = allNodes.Find(n => n.id == tid);
            branchNodes.Add(target);

            if (target != null)
            {
                int idx = branchNodes.Count - 1;
                if (idx < branchButtons.Count && branchButtons[idx] != null)
                {
                    while (branchEdges.Count <= idx) branchEdges.Add(null);
                    if (branchEdges[idx] == null)
                    {
                        var obj = Instantiate(edge);
                        branchEdges[idx] = obj.GetComponent<Edge>();
                        branchEdges[idx].MyNode = this;
                    }
                    branchEdges[idx].SetNameless(
                        branchButtons[idx].transform.position,
                        target.prevButton.transform.position);
                }

                target.PrevNode = this;
                target.prevEdge = branchEdges[idx];
            }
        }
    }

    // ── 삭제 ────────────────────────────────────────────

    public new void Remove()
    {
        // 브랜치 엣지 모두 제거
        foreach (var e in branchEdges)
            if (e != null) e.Remove();

        // 브랜치 노드의 PrevNode 해제
        foreach (var n in branchNodes)
            if (n != null) { n.PrevNode = null; n.prevEdge = null; }

        // 기본 제거
        if (PrevNode != null)
        {
            PrevNode.NextNode = null;
            if (prevEdge != null) prevEdge.Remove();
        }

        NodeGraphManager.instance.nodes.Remove(this);
        Destroy(gameObject);
    }

    // ── 스냅 후 엣지 갱신 ───────────────────────────────

    public new void OnSnapshot()
    {
        Vector2Int gridPos = WorldToRectGrid(transform.position, snap);
        Vector3 snapPos = RectGridToWorld(gridPos, snap);
        SetPosition(snapPos);

        for (int i = 0; i < branchEdges.Count; i++)
        {
            if (branchEdges[i] != null && i < branchNodes.Count && branchNodes[i] != null
                && i < branchButtons.Count && branchButtons[i] != null)
            {
                branchEdges[i].SetNameless(
                    branchButtons[i].transform.position,
                    branchNodes[i].prevButton.transform.position);
            }
        }

        UpdateAllIncomingEdges();
    }
}
