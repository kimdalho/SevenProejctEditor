using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 별도 씬에서 TSV 커맨드를 시뮬레이션하는 컷씬 플레이어.
/// 씬 진입 시 CutSceneData.Commands를 자동 재생한다.
/// ESC → 에디터 씬으로 복귀.
/// </summary>
public class CutScenePlayer : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject cutsceneCanvasPrefab;

    [Header("State")]
    public bool isPlaying;
    public int currentIndex;
    public int totalCount;

    // UI
    private Canvas canvas;
    private DialogueUI dialogueUI;
    private FadeUI fadeUI;
    private CharacterDisplay characterDisplay;
    private TextMeshProUGUI progressTMP;
    private TextMeshProUGUI commandInfoTMP;
    private TextMeshProUGUI hintTMP;

    private Camera cutsceneCam;
    private Coroutine playCoroutine;

    private void Awake()
    {
        BuildCamera();
        EnsureCharacterManager();
        BuildUI();
    }

    /// <summary>
    /// CutScene 씬에는 CharacterManager가 없으므로 자동 생성.
    /// Resources/Characters/ 폴더에서 스프라이트를 로드한다.
    /// </summary>
    private void EnsureCharacterManager()
    {
        if (CharacterManager.instance != null) return;

        var cmObj = new GameObject("CharacterManager");
        cmObj.AddComponent<CharacterManager>();
        // Awake()에서 자동으로 Resources/Characters/ 로드됨
    }

    private void Start()
    {
        // 씬 로드 직후 자동 재생
        if (CutSceneData.Commands != null && CutSceneData.Commands.Count > 0)
        {
            Play(CutSceneData.Commands);
        }
        else
        {
            Debug.LogWarning("[CutScene] 재생할 커맨드가 없습니다. 에디터로 복귀합니다.");
            ReturnToEditor();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Stop();
            ReturnToEditor();
        }
    }

    // ── 씬 구성 ──────────────────────────────────────

    private void BuildCamera()
    {
        var camObj = new GameObject("CutSceneCamera");
        cutsceneCam = camObj.AddComponent<Camera>();
        cutsceneCam.clearFlags = CameraClearFlags.SolidColor;
        cutsceneCam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        cutsceneCam.orthographic = true;
        cutsceneCam.orthographicSize = 5f;
        camObj.AddComponent<AudioListener>();
    }

    private void BuildUI()
    {
        if (cutsceneCanvasPrefab == null)
        {
            Debug.LogWarning("[CutScenePlayer] cutsceneCanvasPrefab이 할당되지 않았습니다. Inspector에서 프리팹을 연결하세요.");
            return;
        }

        var canvasInstance = Instantiate(cutsceneCanvasPrefab, transform);
        canvas = canvasInstance.GetComponent<Canvas>();
        dialogueUI = canvasInstance.GetComponentInChildren<DialogueUI>(true);
        fadeUI = canvasInstance.GetComponentInChildren<FadeUI>(true);
        characterDisplay = canvasInstance.GetComponentInChildren<CharacterDisplay>(true);

        // DebugInfo TMP — 이름으로 검색
        var allTMPs = canvasInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in allTMPs)
        {
            switch (tmp.gameObject.name)
            {
                case "Progress_TMP": progressTMP = tmp; break;
                case "CommandInfo_TMP": commandInfoTMP = tmp; break;
                case "Hint_TMP": hintTMP = tmp; break;
            }
        }

        // 초기 상태
        dialogueUI.Hide();
        fadeUI.SetClear();
        characterDisplay.HideAll();
    }

    // ── 재생 API ─────────────────────────────────────

    public void Play(List<TsvCommand> commands)
    {
        if (isPlaying) return;
        if (commands == null || commands.Count == 0) return;

        isPlaying = true;
        currentIndex = 0;
        totalCount = commands.Count;
        playCoroutine = StartCoroutine(PlayCoroutine(commands));
    }

    public void Stop()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        isPlaying = false;
        dialogueUI.Hide();
        fadeUI.SetClear();
        characterDisplay.HideAll();
    }

    private void ReturnToEditor()
    {
        string returnScene = CutSceneData.ReturnSceneName;
        CutSceneData.Commands = null;
        CutSceneData.ReturnSceneName = null;

        if (!string.IsNullOrEmpty(returnScene))
            SceneManager.LoadScene(returnScene);
        else
            Debug.LogWarning("[CutScene] 복귀할 씬 이름이 없습니다.");
    }

    // ── 재생 코루틴 ──────────────────────────────────

    private IEnumerator PlayCoroutine(List<TsvCommand> commands)
    {
        Debug.Log($"[CutScene] 시뮬레이션 시작 — 총 {commands.Count}개 커맨드");

        for (int i = 0; i < commands.Count; i++)
        {
            currentIndex = i + 1;
            var cmd = commands[i];
            UpdateProgress(cmd);

            yield return ExecuteCommand(cmd);
        }

        Debug.Log("[CutScene] 시뮬레이션 완료 — 아무 키나 눌러 에디터로 복귀");
        progressTMP.text = $"[완료] {totalCount}개 커맨드 재생 끝";
        commandInfoTMP.text = "Space / Enter / 클릭 → 에디터 복귀";

        // 완료 후 입력 대기
        yield return dialogueUI.WaitForInput();

        Stop();
        ReturnToEditor();
    }

    private void UpdateProgress(TsvCommand cmd)
    {
        progressTMP.text = $"[{currentIndex} / {totalCount}]";

        string detail = cmd.StateStr switch
        {
            "say" => $"say  [{cmd.Get("character")}] {Truncate(cmd.Get("str_1"), 30)}",
            "fade" => $"fade  dir={cmd.StateInt} dur={cmd.GetFloat("duration", cmd.GetFloat("wait", 1f))} ease={cmd.Get("easeType", "Linear")}",
            "wait" => $"wait  {cmd.GetFloat("wait", 0f)}s",
            "showcharacter" => $"showcharacter  [{cmd.name}] → {cmd.Get("endLocation", "Mid")} ease={cmd.Get("easeType", "OutCubic")}",
            _ => cmd.StateStr
        };
        commandInfoTMP.text = detail;
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }

    // ── 커맨드 실행 ──────────────────────────────────

    private IEnumerator ExecuteCommand(TsvCommand cmd)
    {
        switch (cmd.StateStr)
        {
            case "say":
                yield return ExecuteSay(cmd);
                break;
            case "wait":
                yield return ExecuteWait(cmd);
                break;
            case "fade":
                yield return ExecuteFade(cmd);
                break;
            case "showcharacter":
                yield return ExecuteShowCharacter(cmd);
                break;
            default:
                Debug.LogWarning($"[CutScene] 알 수 없는 커맨드: {cmd.StateStr} (id={cmd.Id})");
                yield break;
        }
    }

    private IEnumerator ExecuteSay(TsvCommand cmd)
    {
        string character = cmd.Get("character");
        string text = cmd.Get("str_1");
        yield return dialogueUI.ShowDialogue(character, text);
        yield return dialogueUI.WaitForInput();
    }

    private IEnumerator ExecuteWait(TsvCommand cmd)
    {
        float duration = cmd.GetFloat("wait", 1f);
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator ExecuteFade(TsvCommand cmd)
    {
        int dir = cmd.StateInt ?? 1;
        float duration = cmd.GetFloat("duration", cmd.GetFloat("wait", 1f));

        EaseType ease = EaseType.Linear;
        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out ease);

        yield return fadeUI.DoFade(dir, duration, ease);
    }

    private IEnumerator ExecuteShowCharacter(TsvCommand cmd)
    {
        // 표정
        eExpression expression = eExpression.None;
        if (cmd.Has("expression"))
            Enum.TryParse(cmd.Get("expression"), true, out expression);

        // 캐릭터 이름
        string charName = cmd.name;
        if (string.IsNullOrEmpty(charName))
            charName = cmd.Get("character");

        // CharacterManager에서 조회
        CharacterData data = null;
        if (CharacterManager.instance != null && !string.IsNullOrEmpty(charName))
            data = CharacterManager.instance.GetCharacter(charName);

        // 없으면 더미 생성
        if (data == null)
            data = new CharacterData { name = charName ?? "???" };

        // 도착 위치 (Left/Mid/Right)
        eLocation targetLoc = eLocation.Mid;
        if (cmd.Has("endLocation"))
            Enum.TryParse(cmd.Get("endLocation"), true, out targetLoc);

        // 이징 타입
        EaseType ease = EaseType.OutCubic;
        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out ease);

        float duration = cmd.GetFloat("wait", 0.5f);
        yield return characterDisplay.ShowCharacter(data, expression, targetLoc, duration, ease);
    }
}
