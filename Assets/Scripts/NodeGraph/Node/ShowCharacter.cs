using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum eLocation
{
    Left,
    Mid,
    Right,
}

public enum eExpression
{
    None,
    Happy,
    Sad,
    Angry,
    Surprise,
}

[System.Serializable]
public class Ex
{
    public Sprite sprite;
    public eExpression expression;
}

[System.Serializable]
public class CharacterData
{
    public string name;
    public Sprite defaultSprite;
    public List<Ex> expressions = new();

    /// <summary>
    /// 표정에 맞는 스프라이트 반환. 없으면 기본 스프라이트.
    /// </summary>
    public Sprite GetSprite(eExpression exp)
    {
        if (exp != eExpression.None)
        {
            foreach (var e in expressions)
                if (e.expression == exp && e.sprite != null)
                    return e.sprite;
        }
        return defaultSprite;
    }
}

public class ShowCharacter : BaseNode
{
    public CharacterData characterData;
    public eExpression expression;
    public eLocation endLocation;
    public EaseType easeType = EaseType.OutCubic;

    // 에디터 UI (코드로 자동 생성)
    private TextMeshProUGUI charNameLabel;
    private TextMeshProUGUI expressionLabel;
    private TextMeshProUGUI endLocLabel;
    private TextMeshProUGUI easeLabel;
    private TextMeshProUGUI durationLabel;
    private bool uiBuilt;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);

        // 표정
        if (cmd.Has("expression"))
            Enum.TryParse(cmd.Get("expression"), true, out expression);

        // 위치
        if (cmd.Has("endLocation"))
            Enum.TryParse(cmd.Get("endLocation"), true, out endLocation);

        // 이징
        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out easeType);

        // CharacterManager에서 데이터 조회
        string charName = cmd.name;
        if (string.IsNullOrEmpty(charName))
            charName = cmd.Get("character");

        // character 필드 동기화
        if (!string.IsNullOrEmpty(charName) && string.IsNullOrEmpty(character))
            character = charName;

        if (CharacterManager.instance != null && !string.IsNullOrEmpty(charName))
            characterData = CharacterManager.instance.GetCharacter(charName);

        // UI 빌드 (최초 1회)
        if (!uiBuilt)
            BuildShowCharacterUI();

        RefreshLabels();
    }

    // ── 에디터 UI 생성 ─────────────────────────────────

    private void BuildShowCharacterUI()
    {
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) return;

        // 기존 Image(VerticalLayoutGroup) 아래에 새 파라미터 패널 생성
        var containerObj = new GameObject("ShowCharParams");
        containerObj.transform.SetParent(canvas.transform, false);

        var containerRT = containerObj.AddComponent<RectTransform>();
        containerRT.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        containerRT.anchoredPosition = new Vector2(0f, -0.055f);
        containerRT.sizeDelta = new Vector2(0.9f, 1.5f);

        var layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.spacing = -0.02f;
        layout.childAlignment = TextAnchor.UpperCenter;

        // 캐릭터 이름 (녹색, 표시 전용)
        charNameLabel = CreateParamLabel(containerObj.transform, "lbl_char",
            new Color(0.4f, 1f, 0.4f), 0.25f, null);

        // 표정 (노랑, 클릭→순환)
        expressionLabel = CreateParamLabel(containerObj.transform, "lbl_exp",
            new Color(1f, 0.85f, 0.3f), 0.22f,
            () => { CycleEnum(ref expression); RefreshLabels(); SyncToTsvCommand(); });

        // 도착 위치 (파랑, 클릭→ Left/Mid/Right 순환)
        endLocLabel = CreateParamLabel(containerObj.transform, "lbl_to",
            new Color(0.5f, 0.8f, 1f), 0.22f,
            () => { CycleEnum(ref endLocation); RefreshLabels(); SyncToTsvCommand(); });

        // 이징 (주황, 클릭→6종 순환)
        easeLabel = CreateParamLabel(containerObj.transform, "lbl_ease",
            new Color(1f, 0.6f, 0.6f), 0.22f,
            () => { CycleEaseType(); RefreshLabels(); SyncToTsvCommand(); });

        // 시간 (회색, 클릭→프리셋 순환)
        durationLabel = CreateParamLabel(containerObj.transform, "lbl_dur",
            new Color(0.85f, 0.85f, 0.85f), 0.22f,
            () => { CycleDuration(); RefreshLabels(); SyncToTsvCommand(); });

        uiBuilt = true;
    }

    private TextMeshProUGUI CreateParamLabel(Transform parent, string objName,
        Color color, float fontSize, Action onClick)
    {
        var obj = new GameObject(objName);
        obj.transform.SetParent(parent, false);

        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, fontSize + 0.04f);

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = (onClick != null);
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

    // ── UI 업데이트 ─────────────────────────────────────

    private void RefreshLabels()
    {
        if (!uiBuilt) return;

        string charName = character;
        if (string.IsNullOrEmpty(charName))
            charName = tsvCommand?.name ?? "???";

        charNameLabel.text = charName;
        expressionLabel.text = $"Exp: {expression}";
        endLocLabel.text = $"Pos: {endLocation}";
        easeLabel.text = $"Ease: {easeType}";
        durationLabel.text = $"Dur: {wait:F1}s";
    }

    // ── 값 순환 ─────────────────────────────────────────

    private void CycleEnum<T>(ref T value) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int idx = Array.IndexOf(values, value);
        value = values[(idx + 1) % values.Length];
    }

    private void CycleEaseType()
    {
        var values = (EaseType[])Enum.GetValues(typeof(EaseType));
        int idx = Array.IndexOf(values, easeType);
        easeType = values[(idx + 1) % values.Length];
    }

    private void CycleDuration()
    {
        float[] presets = { 0.3f, 0.5f, 0.8f, 1f, 1.5f, 2f, 3f };
        int closest = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < presets.Length; i++)
        {
            float dist = Mathf.Abs(presets[i] - wait);
            if (dist < minDist) { minDist = dist; closest = i; }
        }
        wait = presets[(closest + 1) % presets.Length];
    }

    // ── TSV 동기화 ──────────────────────────────────────

    private void SyncToTsvCommand()
    {
        if (tsvCommand == null) return;
        if (tsvCommand.Fields == null)
            tsvCommand.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        tsvCommand.Fields["expression"] = expression.ToString();
        tsvCommand.Fields["endLocation"] = endLocation.ToString();
        tsvCommand.Fields["easeType"] = easeType.ToString();
        tsvCommand.Fields["wait"] = wait.ToString("F1");

        if (!string.IsNullOrEmpty(character))
            tsvCommand.Fields["character"] = character;
    }

    // ── 런타임 실행 ─────────────────────────────────────

    public override IEnumerator Execute(NodePlayer player)
    {
        var display = player.characterDisplay;
        float duration = wait > 0f ? wait : 0.5f;
        yield return display.ShowCharacter(characterData, expression, endLocation, duration, easeType);
    }
}
