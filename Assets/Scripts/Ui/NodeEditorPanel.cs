using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 노드 클릭 시 해당 타입 그룹을 열어 데이터를 표시/편집.
/// UI는 씬에 완전히 배치되어 있고 스크립트는 show/hide + 데이터 바인딩만 담당.
/// </summary>
public class NodeEditorPanel : MonoBehaviour
{
    public static NodeEditorPanel instance;

    [Header("공통")]
    public TextMeshProUGUI titleText;
    public Button          closeButton;

    // ── Say ───────────────────────────────────────────────
    [Header("Say")]
    public GameObject        sayGroup;
    public NodeEditorListItem sayCharItem;
    public NodeEditorListItem sayStr1Item;

    // ── ShowCharacter ─────────────────────────────────────
    [Header("ShowCharacter")]
    public GameObject        showCharGroup;
    public NodeEditorListItem scCharItem;
    public NodeEditorListItem scExprItem;
    public NodeEditorListItem scLocItem;
    public NodeEditorListItem scEaseItem;
    public NodeEditorListItem scWaitItem;

    // ── Fade ──────────────────────────────────────────────
    [Header("Fade")]
    public GameObject        fadeGroup;
    public NodeEditorListItem fadeDirItem;
    public NodeEditorListItem fadeEaseItem;
    public NodeEditorListItem fadeDurItem;

    // ── Wait ──────────────────────────────────────────────
    [Header("Wait")]
    public GameObject        waitGroup;
    public NodeEditorListItem waitDurItem;

    // ── Choice ────────────────────────────────────────────
    [Header("Choice")]
    public GameObject         choiceGroup;
    public NodeEditorListItem choiceCountItem;
    public NodeEditorListItem[] choiceOptItems  = new NodeEditorListItem[4];
    public NodeEditorListItem[] choiceTargItems = new NodeEditorListItem[4];

    // ── 내부 ──────────────────────────────────────────────
    private BaseNode currentNode;

    private GameObject[] allGroups;

    // ── 생명주기 ──────────────────────────────────────────

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        allGroups = new[] { sayGroup, showCharGroup, fadeGroup, waitGroup, choiceGroup };

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        HideAllGroups();
        gameObject.SetActive(false);
    }

    // ── 공개 API ──────────────────────────────────────────

    public void Open(BaseNode node)
    {
        currentNode = node;
        gameObject.SetActive(true);
        HideAllGroups();
        Bind();
    }

    public void Close()
    {
        currentNode = null;
        gameObject.SetActive(false);
    }

    // ── 데이터 바인딩 ─────────────────────────────────────

    private void Bind()
    {
        if (currentNode == null) return;
        if (titleText != null)
            titleText.text = $"[{currentNode.tsvCommand?.StateStr ?? "Node"}]  ID {currentNode.id}";

        if      (currentNode is Say          say)    BindSay(say);
        else if (currentNode is ShowCharacter sc)    BindShowChar(sc);
        else if (currentNode is Fade          fade)  BindFade(fade);
        else if (currentNode is Wait          wait)  BindWait(wait);
        else if (currentNode is Choice        choice) BindChoice(choice);
    }

    // ── Say ───────────────────────────────────────────────

    private void BindSay(Say say)
    {
        sayGroup?.SetActive(true);

        sayCharItem?.SetupInput("캐릭터", say.character ?? "", false, val =>
        {
            say.character = val;
            SyncField(say.tsvCommand, "character", val);
        });

        sayStr1Item?.SetupInput("대사", say.str_1 ?? "", true, val =>
        {
            say.str_1 = val;
            SyncField(say.tsvCommand, "str_1", val);
        });
    }

    // ── ShowCharacter ──────────────────────────────────────

    private void BindShowChar(ShowCharacter sc)
    {
        showCharGroup?.SetActive(true);

        scCharItem?.SetupInput("캐릭터", sc.character ?? "", false, val =>
        {
            sc.character = val;
            if (CharacterManager.instance != null && !string.IsNullOrEmpty(val))
                sc.characterData = CharacterManager.instance.GetCharacter(val);
            SyncShowChar(sc);
        });

        BindEnumDropdown(scExprItem, "표정", sc.expression, typeof(eExpression),
            val => { sc.expression = (eExpression)val; SyncShowChar(sc); });

        BindEnumDropdown(scLocItem, "위치", sc.endLocation, typeof(eLocation),
            val => { sc.endLocation = (eLocation)val; SyncShowChar(sc); });

        BindEnumDropdown(scEaseItem, "EaseType", sc.easeType, typeof(EaseType),
            val => { sc.easeType = (EaseType)val; SyncShowChar(sc); });

        scWaitItem?.SetupInput("시간(초)", sc.wait.ToString("F1"), false, val =>
        {
            if (float.TryParse(val, out float f)) { sc.wait = f; SyncShowChar(sc); }
        });
    }

    // ── Fade ──────────────────────────────────────────────

    private void BindFade(Fade fade)
    {
        fadeGroup?.SetActive(true);

        RefreshFadeDir(fade);

        BindEnumDropdown(fadeEaseItem, "EaseType", fade.easeType, typeof(EaseType),
            val => { fade.easeType = (EaseType)val; SyncFade(fade); });

        fadeDurItem?.SetupInput("시간(초)", fade.wait.ToString("F1"), false, val =>
        {
            if (float.TryParse(val, out float f)) { fade.wait = f; SyncFade(fade); }
        });
    }

    private void RefreshFadeDir(Fade fade)
    {
        int dir = fade.tsvCommand?.StateInt ?? 1;
        fadeDirItem?.SetupEnum("방향", new[] { "FadeOut", "FadeIn" }, dir == 2 ? 1 : 0,
            idx =>
            {
                if (fade.tsvCommand != null)
                    fade.tsvCommand.StateInt = idx == 1 ? 2 : 1;
            });
    }

    // ── Wait ──────────────────────────────────────────────

    private void BindWait(Wait wait)
    {
        waitGroup?.SetActive(true);

        waitDurItem?.SetupInput("대기(초)", wait.wait.ToString("F1"), false, val =>
        {
            if (float.TryParse(val, out float f))
            {
                wait.wait = f;
                SyncField(wait.tsvCommand, "wait", f.ToString("F1"));
            }
        });
    }

    // ── Choice ────────────────────────────────────────────

    private void BindChoice(Choice choice)
    {
        choiceGroup?.SetActive(true);
        RefreshChoiceCount(choice);
    }

    private void RefreshChoiceCount(Choice choice)
    {
        int cnt = choice.options.Count;

        choiceCountItem?.SetupEnum("선택지 수", new[] { "2", "3", "4" }, Mathf.Clamp(cnt - 2, 0, 2), idx =>
        {
            int c = idx + 2;
            while (choice.options.Count < c) choice.options.Add("");
            while (choice.options.Count > c) choice.options.RemoveAt(choice.options.Count - 1);
            for (int i = 0; i < choice.branchButtons.Count; i++)
                if (choice.branchButtons[i] != null)
                    choice.branchButtons[i].gameObject.SetActive(i < choice.options.Count);
            SyncChoice(choice);
            RefreshChoiceCount(choice);
        });

        // 옵션 항목: 개수만큼 활성화
        for (int i = 0; i < 4; i++)
        {
            if (choiceOptItems == null || i >= choiceOptItems.Length) break;
            var optItem = choiceOptItems[i];
            if (optItem == null) continue;

            if (i < cnt)
            {
                int idx = i;
                optItem.gameObject.SetActive(true);
                optItem.SetupInput($"선택지 {i + 1}", choice.options[i], false, val =>
                {
                    while (choice.options.Count <= idx) choice.options.Add("");
                    choice.options[idx] = val;
                    SyncChoice(choice);
                });
            }
            else
            {
                optItem.gameObject.SetActive(false);
            }
        }

        // 분기 연결 표시
        for (int i = 0; i < 4; i++)
        {
            if (choiceTargItems == null || i >= choiceTargItems.Length) break;
            var targItem = choiceTargItems[i];
            if (targItem == null) continue;

            if (i < cnt)
            {
                string target = (i < choice.branchNodes.Count && choice.branchNodes[i] != null)
                    ? $"ID {choice.branchNodes[i].id}" : "없음";
                targItem.gameObject.SetActive(true);
                targItem.SetupReadOnly($"선택지 {i + 1} →", target);
            }
            else
            {
                targItem.gameObject.SetActive(false);
            }
        }
    }

    // ── Enum 드롭다운 바인딩 ─────────────────────────────

    private static void BindEnumDropdown(NodeEditorListItem item, string label,
        Enum current, Type enumType, Action<int> onChange)
    {
        if (item == null) return;
        var values = Enum.GetValues(enumType);
        var names  = Enum.GetNames(enumType);
        int currentIdx = Math.Max(0, Array.IndexOf(values, current));

        item.SetupEnum(label, names, currentIdx,
            idx => onChange?.Invoke((int)values.GetValue(idx)));
    }

    // ── TSV 동기화 ────────────────────────────────────────

    private static void SyncField(TsvCommand cmd, string key, string value)
    {
        if (cmd == null) return;
        if (cmd.Fields == null) cmd.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        cmd.Fields[key] = value;
    }

    private static void SyncShowChar(ShowCharacter sc)
    {
        if (sc.tsvCommand == null) return;
        if (sc.tsvCommand.Fields == null)
            sc.tsvCommand.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        sc.tsvCommand.Fields["character"]   = sc.character    ?? "";
        sc.tsvCommand.Fields["expression"]  = sc.expression.ToString();
        sc.tsvCommand.Fields["endLocation"] = sc.endLocation.ToString();
        sc.tsvCommand.Fields["easeType"]    = sc.easeType.ToString();
        sc.tsvCommand.Fields["wait"]        = sc.wait.ToString("F1");
    }

    private static void SyncFade(Fade fade)
    {
        if (fade.tsvCommand == null) return;
        if (fade.tsvCommand.Fields == null)
            fade.tsvCommand.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        fade.tsvCommand.Fields["easeType"] = fade.easeType.ToString();
        fade.tsvCommand.Fields["duration"] = fade.wait.ToString("F1");
    }

    private static void SyncChoice(Choice choice)
    {
        if (choice.tsvCommand == null) return;
        if (choice.tsvCommand.Fields == null)
            choice.tsvCommand.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        choice.tsvCommand.Fields["options"] = string.Join("|", choice.options);
        var ids = new List<string>();
        for (int i = 0; i < choice.options.Count; i++)
            ids.Add(i < choice.branchNodes.Count && choice.branchNodes[i] != null
                ? choice.branchNodes[i].id.ToString() : "-1");
        choice.tsvCommand.Fields["targets"] = string.Join("|", ids);
        choice.tsvCommand.StateInt = choice.options.Count;
    }

    // ── 유틸 ──────────────────────────────────────────────

    private void HideAllGroups()
    {
        if (allGroups == null) return;
        foreach (var g in allGroups)
            if (g != null) g.SetActive(false);
    }
}
