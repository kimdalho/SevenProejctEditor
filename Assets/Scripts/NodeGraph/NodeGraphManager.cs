using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum eNodeType
{
    None,
    Say,
    Fade,
    CharacterShow
}

//��� Ÿ���� ���� ���������̺��� �����͸� �޴µ� Ÿ�Կ����� ���� �ʾƾ��Ѵ�.
public interface INodeDataGetService
{
    public void SetData();
}


public class NodeGraphManager : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCam;

    [Header("Prefabs")]
    public BaseNode NodePrefab;
    public BaseNode NodePrefab1;
    public BaseNode NodePrefab2;
    public BaseNode NodePrefab3;
    public BaseNode NodePrefab4;
    public BaseNode NodePrefab5;   // Choice
    public static NodeGraphManager instance;
    public List<BaseNode> nodes = new();


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        mainCam = Camera.main;

        // NodePlayer 자동 생성
        if (NodePlayer.instance == null)
        {
            var playerObj = new GameObject("NodePlayer");
            playerObj.AddComponent<NodePlayer>();
        }

        // CutScene 씬 이름 (Build Settings에 추가 필요)
        cutSceneSceneName = "CutScene";
    }

    private void Start()
    {
        // CutScene에서 복귀한 경우 → 에디터 노드 상태 복원
        RestoreEditorState();
    }

    /// <summary>
    /// CutScene 복귀 시 저장된 에디터 상태로 노드 그래프를 재생성한다.
    /// </summary>
    private void RestoreEditorState()
    {
        if (CutSceneData.EditorCommands == null || CutSceneData.EditorCommands.Count == 0)
            return;

        // StoryDatabase에 복원
        if (StoryDatabase.instance != null)
        {
            StoryDatabase.instance.Commands = CutSceneData.EditorCommands;
            if (!string.IsNullOrEmpty(CutSceneData.EditorResourcesPath))
                StoryDatabase.instance.resourcesPath = CutSceneData.EditorResourcesPath;
        }

        // 노드 그래프 재생성
        CreateNodeToTsvCommand();
        Debug.Log($"[NodeGraph] CutScene 복귀 — {CutSceneData.EditorCommands.Count}개 노드 복원 완료");

        // 복원 데이터 정리
        CutSceneData.EditorCommands = null;
        CutSceneData.EditorResourcesPath = null;
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.N))
        //{
        //    CreateNodeToType(eNodeType.None, NodePrefab);
        //}
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    CreateNodeToType(eNodeType.Say, NodePrefab1);
        //}
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    CreateNodeToType(eNodeType.Fade, NodePrefab2);
        //}
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    CreateNodeToType(eNodeType.CharacterShow, NodePrefab3);
        //}
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    CreateNodeToType(eNodeType.None, NodePrefab4);
        //}

        // ---- CutScene (F7 → 별도 씬) ----
        if (Input.GetKeyDown(KeyCode.F7))
        {
            LaunchCutScene();
            return;
        }

        // ---- Runtime Player (Space) ----
        if (NodePlayer.instance != null)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !NodePlayer.instance.isPlaying)
            {
                NodePlayer.instance.Play();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) && NodePlayer.instance.isPlaying)
            {
                NodePlayer.instance.Stop();
                return;
            }
        }

        // 재생 중에는 에디터 입력 차단
        if (NodePlayer.instance != null && NodePlayer.instance.isPlaying)
            return;

        // InputField에 타이핑 중이면 에디터 단축키 무시
        var selected = EventSystem.current?.currentSelectedGameObject;
        bool isTyping = selected != null && selected.GetComponentInParent<TMP_InputField>() != null;

        if (!isTyping)
        {
            // 삭제
            if(Input.GetKeyDown(KeyCode.X))
            {
                selectNode.Remove();
            }

            if(Input.GetKey(KeyCode.S))
            {
                var camPos = mainCam.transform.position;
                camPos.z -= 0.1f;
                mainCam.transform.position = camPos;
            }
            if (Input.GetKey(KeyCode.W))
            {
                var camPos = mainCam.transform.position;
                camPos.z += 0.1f;
                mainCam.transform.position = camPos;
            }
            if (Input.GetKey(KeyCode.A))
            {
                var camPos = mainCam.transform.position;
                camPos.x -= 0.1f;
                mainCam.transform.position = camPos;
            }
            if (Input.GetKey(KeyCode.D))
            {
                var camPos = mainCam.transform.position;
                camPos.x += 0.1f;
                mainCam.transform.position = camPos;
            }
        }

        // 마우스 휠 확대/축소
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
        {
            mainCam.orthographicSize -= scroll * 0.5f;
            mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize, 0.5f, 20f);
        }



        // F5: TSV → 노드 그래프 생성
        if (Input.GetKeyDown(KeyCode.F5))
        {
            CreateNodeToTsvCommand();
        }

        // F6: 노드 → TSV 저장
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SaveTSV();
        }

    }

    //----------------- ��� ���� ------------------------------------------

    public void CreateNodeToType(eNodeType type,BaseNode prefab)
    {
        var obj = Instantiate(prefab);
        var newbaseNode = obj.GetComponent<BaseNode>();
        newbaseNode.transform.position = Vector3.zero;
        nodes.Add(newbaseNode);
    }

    [Header("CutScene")]
    public string cutSceneSceneName = "CutScene";

    [Header("Layout")]
    public int nodesPerRow = 6;
    public float spacingX = 1.5f;
    public float spacingZ = 1.2f;
    public Vector3 startPos = new Vector3(-4f, 0, 3f);

    [Header("Logic")]
    public BaseNode selectNode;

    public void CreateNodeToTsvCommand()
    {
        // 1) 노드 생성
        var data = StoryDatabase.instance.Commands;
        for (int i = 0; i < data.Count; i++)
        {
            BaseNode node = null;
            switch (data[i].StateStr)
            {
                case "say":
                    node = Instantiate(NodePrefab1);
                    break;
                case "fade":
                    node = Instantiate(NodePrefab2);
                    break;
                case "wait":
                    node = Instantiate(NodePrefab3);
                    break;
                case "showcharacter":
                    node = Instantiate(NodePrefab4);
                    break;
                case "choice":
                    node = Instantiate(NodePrefab5);
                    break;
                default:
                    Debug.LogWarning($"[NodeGraph] 알 수 없는 타입: {data[i].StateStr}");
                    continue;
            }

            node.SetCommnadData(data[i]);

            // 왼→오, 위→아래 흐름형 배치
            int col = nodes.Count % nodesPerRow;
            int row = nodes.Count / nodesPerRow;
            node.transform.position = new Vector3(
                startPos.x + col * spacingX,
                0f,
                startPos.z - row * spacingZ
            );

            nodes.Add(node);
        }

        // 2) 자동 링크
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            if (nodes[i].id <= -1) return;
            nodes[i].SetLinkNext(nodes[i + 1]);
        }

        // 3) Choice 노드 분기 타겟 복원
        foreach (var n in nodes)
        {
            if (n is Choice choice)
            {
                choice.ResolveBranchTargets(nodes);
                // Choice의 기본 NextNode(자동링크된 것) 제거
                if (choice.NextNode != null)
                {
                    choice.NextNode.PrevNode = null;
                    choice.NextNode.prevEdge = null;
                }
                if (choice.nextEdge != null)
                {
                    choice.nextEdge.Remove();
                    choice.nextEdge = null;
                }
                choice.NextNode = null;
            }
        }
    }

    public void SaveTSV()
    {
        if (nodes == null || nodes.Count == 0)
        {
            Debug.LogWarning("[NodeGraph] 저장할 노드가 없습니다.");
            return;
        }

        // 1) 체인 순서대로 노드 수집 (PrevNode==null인 시작 노드부터)
        var ordered = new List<BaseNode>();
        BaseNode start = null;
        foreach (var n in nodes)
        {
            if (n.PrevNode == null)
            {
                start = n;
                break;
            }
        }
        if (start == null) start = nodes[0];

        var visited = new HashSet<BaseNode>();
        CollectDFS(start, ordered, visited);

        // 체인에 포함되지 않은 노드도 끝에 추가
        foreach (var n in nodes)
        {
            if (!visited.Contains(n))
                ordered.Add(n);
        }

        // 2) 모든 노드의 Fields 키 수집 (헤더 구성)
        var extraKeys = new List<string>();
        var keySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var n in ordered)
        {
            if (n.tsvCommand?.Fields == null) continue;
            foreach (var key in n.tsvCommand.Fields.Keys)
            {
                if (keySet.Add(key))
                    extraKeys.Add(key);
            }
        }

        // character, str_1은 항상 포함
        if (!keySet.Contains("character")) { extraKeys.Insert(0, "character"); keySet.Add("character"); }
        if (!keySet.Contains("str_1")) { extraKeys.Insert(keySet.Contains("character") ? 1 : 0, "str_1"); keySet.Add("str_1"); }

        // ShowCharacter 전용 키 보장
        bool hasShowChar = false;
        bool hasFade = false;
        foreach (var n in ordered)
        {
            if (n is ShowCharacter) hasShowChar = true;
            if (n is Fade) hasFade = true;
        }
        if (hasShowChar)
        {
            string[] scKeys = { "expression", "endLocation", "easeType" };
            foreach (var k in scKeys)
            {
                if (keySet.Add(k))
                    extraKeys.Add(k);
            }
        }
        if (hasFade)
        {
            if (keySet.Add("easeType"))
                extraKeys.Add("easeType");
        }

        // Choice 전용 키 보장
        bool hasChoice = false;
        foreach (var n in ordered)
        {
            if (n is Choice) { hasChoice = true; break; }
        }
        if (hasChoice)
        {
            if (keySet.Add("options")) extraKeys.Add("options");
            if (keySet.Add("targets")) extraKeys.Add("targets");
        }

        // 3) TSV 문자열 작성
        var sb = new StringBuilder();

        // 헤더
        sb.Append("Id\tstate_str\tstate_int");
        foreach (var key in extraKeys)
            sb.Append('\t').Append(key);
        sb.AppendLine();

        // 데이터 행
        foreach (var n in ordered)
        {
            string stateStr = GetStateStr(n);
            string stateInt = (n.tsvCommand?.StateInt.HasValue == true)
                ? n.tsvCommand.StateInt.Value.ToString()
                : "";

            sb.Append(n.id).Append('\t');
            sb.Append(stateStr).Append('\t');
            sb.Append(stateInt);

            // 파생 노드 타입 캐스팅
            var sc = n as ShowCharacter;
            var fade = n as Fade;
            var choice = n as Choice;

            foreach (var key in extraKeys)
            {
                sb.Append('\t');
                // 우선 현재 노드 필드에서, 없으면 BaseNode 기본 필드에서
                if (key.Equals("character", StringComparison.OrdinalIgnoreCase))
                    sb.Append(n.character ?? "");
                else if (key.Equals("str_1", StringComparison.OrdinalIgnoreCase))
                    sb.Append(n.str_1 ?? "");
                else if (key.Equals("wait", StringComparison.OrdinalIgnoreCase))
                    sb.Append(n.wait);
                // ShowCharacter 전용
                else if (sc != null && key.Equals("expression", StringComparison.OrdinalIgnoreCase))
                    sb.Append(sc.expression.ToString());
                else if (sc != null && key.Equals("endLocation", StringComparison.OrdinalIgnoreCase))
                    sb.Append(sc.endLocation.ToString());
                // easeType (ShowCharacter, Fade 공용)
                else if (sc != null && key.Equals("easeType", StringComparison.OrdinalIgnoreCase))
                    sb.Append(sc.easeType.ToString());
                else if (fade != null && key.Equals("easeType", StringComparison.OrdinalIgnoreCase))
                    sb.Append(fade.easeType.ToString());
                // Choice 전용
                else if (choice != null && key.Equals("options", StringComparison.OrdinalIgnoreCase))
                    sb.Append(string.Join("|", choice.options));
                else if (choice != null && key.Equals("targets", StringComparison.OrdinalIgnoreCase))
                {
                    var ids = new List<string>();
                    foreach (var bn in choice.branchNodes)
                        ids.Add(bn != null ? bn.id.ToString() : "-1");
                    sb.Append(string.Join("|", ids));
                }
                else
                    sb.Append(n.tsvCommand?.Get(key, "") ?? "");
            }
            sb.AppendLine();
        }

        // 4) 파일 저장 (원본이름_날짜시간.tsv)
        string originalName = Path.GetFileNameWithoutExtension(StoryDatabase.instance.resourcesPath);
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{originalName}_{timestamp}.tsv";

        string dir = Path.Combine(Application.dataPath, "Resources", "Story");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string fullPath = Path.Combine(dir, fileName);
        File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[NodeGraph] TSV 저장 완료: {fullPath}");
    }

    /// <summary>
    /// F7: 현재 TSV 데이터를 CutScene 씬으로 넘겨서 시뮬레이션
    /// </summary>
    public void LaunchCutScene()
    {
        // 커맨드 소스: 노드가 있으면 노드 체인, 없으면 StoryDatabase 원본
        List<TsvCommand> commands;

        if (nodes != null && nodes.Count > 0)
        {
            // 노드 체인 순서대로 TsvCommand 수집
            commands = new List<TsvCommand>();
            BaseNode start = null;
            foreach (var n in nodes)
            {
                if (n.PrevNode == null) { start = n; break; }
            }
            if (start == null) start = nodes[0];

            // DFS로 모든 분기 포함
            var orderedNodes = new List<BaseNode>();
            var visited = new HashSet<BaseNode>();
            CollectDFS(start, orderedNodes, visited);

            foreach (var current in orderedNodes)
            {
                // tsvCommand가 있으면 그대로, 없으면 현재 필드로 생성
                var cmd = current.tsvCommand ?? new TsvCommand
                {
                    Id = current.id,
                    StateStr = GetStateStr(current),
                    Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["character"] = current.character ?? "",
                        ["str_1"] = current.str_1 ?? ""
                    }
                };

                // 파생 노드: 에디터에서 편집한 값을 cmd에 동기화
                if (cmd.Fields == null)
                    cmd.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (current is ShowCharacter sc)
                {
                    cmd.Fields["expression"] = sc.expression.ToString();
                    cmd.Fields["endLocation"] = sc.endLocation.ToString();
                    cmd.Fields["easeType"] = sc.easeType.ToString();
                    cmd.Fields["wait"] = sc.wait.ToString("F1");
                    if (!string.IsNullOrEmpty(sc.character))
                        cmd.Fields["character"] = sc.character;
                }
                else if (current is Fade fade)
                {
                    cmd.Fields["easeType"] = fade.easeType.ToString();
                    cmd.Fields["duration"] = fade.wait.ToString("F1");
                }
                else if (current is Choice choiceNode)
                {
                    cmd.Fields["options"] = string.Join("|", choiceNode.options);
                    var ids = new List<string>();
                    foreach (var bn in choiceNode.branchNodes)
                        ids.Add(bn != null ? bn.id.ToString() : "-1");
                    cmd.Fields["targets"] = string.Join("|", ids);
                    cmd.StateInt = choiceNode.options.Count;
                }

                commands.Add(cmd);
            }
        }
        else if (StoryDatabase.instance != null && StoryDatabase.instance.Commands.Count > 0)
        {
            commands = StoryDatabase.instance.Commands;
        }
        else
        {
            Debug.LogWarning("[CutScene] 재생할 데이터가 없습니다.");
            return;
        }

        // 에디터 노드 상태 보존 (복귀 시 복원용)
        CutSceneData.EditorCommands = new List<TsvCommand>(commands);
        if (StoryDatabase.instance != null)
            CutSceneData.EditorResourcesPath = StoryDatabase.instance.resourcesPath;

        // static 컨테이너에 재생 데이터 전달
        CutSceneData.Commands = commands;
        CutSceneData.ReturnSceneName = SceneManager.GetActiveScene().name;

        // 씬 존재 여부 확인
        if (Application.CanStreamedLevelBeLoaded(cutSceneSceneName))
        {
            Debug.Log($"[CutScene] {commands.Count}개 커맨드 → {cutSceneSceneName} 씬 로드");
            SceneManager.LoadScene(cutSceneSceneName);
        }
        else
        {
            Debug.LogError($"[CutScene] '{cutSceneSceneName}' 씬이 Build Settings에 없습니다!\n" +
                           "메뉴: Tools > CutScene > Create CutScene Scene 을 실행하세요.");
            CutSceneData.Commands = null;
            CutSceneData.EditorCommands = null;
        }
    }

    private string GetStateStr(BaseNode node)
    {
        if (node.tsvCommand != null && !string.IsNullOrEmpty(node.tsvCommand.StateStr))
            return node.tsvCommand.StateStr;

        if (node is Choice) return "choice";
        if (node is Say) return "say";
        if (node is Fade) return "fade";
        if (node is Wait) return "wait";
        if (node is ShowCharacter) return "showcharacter";
        return "unknown";
    }

    /// <summary>
    /// DFS로 모든 분기를 포함하여 노드를 수집한다.
    /// Choice 노드는 branchNodes를 따라가고, 일반 노드는 NextNode를 따라간다.
    /// </summary>
    private void CollectDFS(BaseNode node, List<BaseNode> ordered, HashSet<BaseNode> visited)
    {
        if (node == null || visited.Contains(node)) return;
        visited.Add(node);
        ordered.Add(node);

        if (node is Choice choice)
        {
            foreach (var branch in choice.branchNodes)
                CollectDFS(branch, ordered, visited);
        }
        else
        {
            CollectDFS(node.NextNode, ordered, visited);
        }
    }
}
