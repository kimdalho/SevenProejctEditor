using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 별도 씬에서 TSV 커맨드를 시뮬레이션하는 컷씬 플레이어.
/// 씬에 배치된 UI/카메라 오브젝트를 직접 Inspector에서 참조.
/// ESC → 에디터 씬으로 복귀.
/// </summary>
public class CutScenePlayer : MonoBehaviour
{
    [Header("UI — 씬에 배치된 오브젝트 연결")]
    public DialogueUI      dialogueUI;
    public FadeUI          fadeUI;
    public CharacterDisplay characterDisplay;
    public ChoiceUI        choiceUI;

    [Header("State")]
    public bool isPlaying;
    public int  currentIndex;
    public int  totalCount;

    private Coroutine playCoroutine;

    private void Awake()
    {
        EnsureCharacterManager();
    }

    private void Start()
    {
        // 미연결 컴포넌트 자동 탐색
        if (dialogueUI       == null) dialogueUI       = FindObjectOfType<DialogueUI>(true);
        if (fadeUI           == null) fadeUI           = FindObjectOfType<FadeUI>(true);
        if (characterDisplay == null) characterDisplay = FindObjectOfType<CharacterDisplay>(true);
        if (choiceUI         == null) choiceUI         = FindObjectOfType<ChoiceUI>(true);

        if (dialogueUI == null)
        {
            Debug.LogError("[CutScenePlayer] DialogueUI를 찾을 수 없습니다. 씬에 배치했는지 확인하세요.");
            return;
        }

        // 초기 상태
        dialogueUI.Hide();
        if (fadeUI           != null) fadeUI.SetClear();
        if (characterDisplay != null) characterDisplay.HideAll();
        if (choiceUI         != null) choiceUI.Hide();

        if (CutSceneData.Commands != null && CutSceneData.Commands.Count > 0)
            Play(CutSceneData.Commands);
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

    private void EnsureCharacterManager()
    {
        if (CharacterManager.instance != null) return;
        var cmObj = new GameObject("CharacterManager");
        cmObj.AddComponent<CharacterManager>();
    }

    // ── 재생 API ─────────────────────────────────────

    public void Play(List<TsvCommand> commands)
    {
        if (isPlaying) return;
        if (commands == null || commands.Count == 0) return;

        isPlaying    = true;
        currentIndex = 0;
        totalCount   = commands.Count;
        playCoroutine = StartCoroutine(PlayCoroutine(commands));
    }

    public void Stop()
    {
        if (playCoroutine != null) { StopCoroutine(playCoroutine); playCoroutine = null; }

        isPlaying = false;
        dialogueUI?.Hide();
        fadeUI?.SetClear();
        characterDisplay?.HideAll();
        choiceUI?.Hide();
    }

    private void ReturnToEditor()
    {
        string returnScene = CutSceneData.ReturnSceneName;
        CutSceneData.Commands      = null;
        CutSceneData.ReturnSceneName = null;

        if (!string.IsNullOrEmpty(returnScene))
            SceneManager.LoadScene(returnScene);
        else
            Debug.LogWarning("[CutScene] 복귀할 씬 이름이 없습니다.");
    }

    // ── 재생 코루틴 ──────────────────────────────────

    private IEnumerator PlayCoroutine(List<TsvCommand> commands)
    {
        Debug.Log($"[CutScene] 시작 — 총 {commands.Count}개 커맨드");

        var idToIndex = new Dictionary<int, int>();
        for (int i = 0; i < commands.Count; i++)
            idToIndex[commands[i].Id] = i;

        int idx = 0;
        while (idx >= 0 && idx < commands.Count)
        {
            currentIndex = idx + 1;
            var cmd = commands[idx];

            if (cmd.StateStr == "choice")
            {
                yield return ExecuteChoice(cmd);
                int targetId = GetChoiceTargetId(cmd, choiceUI.SelectedIndex);
                if (targetId >= 0 && idToIndex.TryGetValue(targetId, out int targetIdx))
                    idx = targetIdx;
                else
                    break;
            }
            else
            {
                yield return ExecuteCommand(cmd);
                idx++;
            }
        }

        Debug.Log("[CutScene] 완료");
        yield return dialogueUI.WaitForInput();
        Stop();
        ReturnToEditor();
    }

    // ── 커맨드 실행 ──────────────────────────────────

    private IEnumerator ExecuteCommand(TsvCommand cmd)
    {
        switch (cmd.StateStr)
        {
            case "say":           yield return ExecuteSay(cmd);           break;
            case "wait":          yield return ExecuteWait(cmd);          break;
            case "fade":          yield return ExecuteFade(cmd);          break;
            case "showcharacter": yield return ExecuteShowCharacter(cmd); break;
            default:
                Debug.LogWarning($"[CutScene] 알 수 없는 커맨드: {cmd.StateStr} (id={cmd.Id})");
                break;
        }
    }

    private IEnumerator ExecuteSay(TsvCommand cmd)
    {
        yield return dialogueUI.ShowDialogue(cmd.Get("character"), cmd.Get("str_1"));
        yield return dialogueUI.WaitForInput();
    }

    private IEnumerator ExecuteWait(TsvCommand cmd)
    {
        yield return new WaitForSeconds(cmd.GetFloat("wait", 1f));
    }

    private IEnumerator ExecuteFade(TsvCommand cmd)
    {
        if (fadeUI == null) yield break;
        int dir = cmd.StateInt ?? 1;
        float duration = cmd.GetFloat("duration", cmd.GetFloat("wait", 1f));
        EaseType ease = EaseType.Linear;
        if (cmd.Has("easeType")) Enum.TryParse(cmd.Get("easeType"), true, out ease);
        yield return fadeUI.DoFade(dir, duration, ease);
    }

    private IEnumerator ExecuteShowCharacter(TsvCommand cmd)
    {
        if (characterDisplay == null) yield break;

        string charName = cmd.Get("character");
        CharacterData data = null;
        if (CharacterManager.instance != null && !string.IsNullOrEmpty(charName))
            data = CharacterManager.instance.GetCharacter(charName);
        if (data == null) data = new CharacterData { name = charName ?? "???" };

        eExpression expression = eExpression.None;
        if (cmd.Has("expression")) Enum.TryParse(cmd.Get("expression"), true, out expression);

        eLocation targetLoc = eLocation.Mid;
        if (cmd.Has("endLocation")) Enum.TryParse(cmd.Get("endLocation"), true, out targetLoc);

        EaseType ease = EaseType.OutCubic;
        if (cmd.Has("easeType")) Enum.TryParse(cmd.Get("easeType"), true, out ease);

        yield return characterDisplay.ShowCharacter(data, expression, targetLoc, cmd.GetFloat("wait", 0.5f), ease);
    }

    private IEnumerator ExecuteChoice(TsvCommand cmd)
    {
        if (choiceUI == null) yield break;
        var options = new List<string>(cmd.Get("options").Split('|'));
        yield return choiceUI.ShowChoices(options);
    }

    private int GetChoiceTargetId(TsvCommand cmd, int selectedIndex)
    {
        string targetStr = cmd.Get("targets");
        if (string.IsNullOrEmpty(targetStr)) return -1;
        var parts = targetStr.Split('|');
        if (selectedIndex < 0 || selectedIndex >= parts.Length) return -1;
        return int.TryParse(parts[selectedIndex].Trim(), out int id) ? id : -1;
    }
}
