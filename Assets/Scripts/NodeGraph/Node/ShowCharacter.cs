using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    // 에디터 UI
    private TMP_InputField characterInput;
    private TextMeshProUGUI expressionLabel;
    private TextMeshProUGUI endLocLabel;
    private TextMeshProUGUI easeLabel;
    private TMP_InputField durationInput;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);

        // ShowCharacter 전용 파싱
        if (cmd.Has("expression"))
            Enum.TryParse(cmd.Get("expression"), true, out expression);

        if (cmd.Has("endLocation"))
            Enum.TryParse(cmd.Get("endLocation"), true, out endLocation);

        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out easeType);

        // CharacterManager에서 데이터 조회
        string charName = cmd.name;
        if (string.IsNullOrEmpty(charName))
            charName = cmd.Get("character");

        if (!string.IsNullOrEmpty(charName) && string.IsNullOrEmpty(character))
            character = charName;

        if (CharacterManager.instance != null && !string.IsNullOrEmpty(charName))
            characterData = CharacterManager.instance.GetCharacter(charName);

        RefreshNodeUI();
    }

    // ── 에디터 UI 생성 ─────────────────────────────────

    protected override void BuildNodeUI()
    {
        var container = GetOrCreateContentContainer();
        if (container == null) return;

        // 캐릭터 이름 (직접 편집 가능)
        characterInput = CreateTMPInputField(container, "input_char",
            character ?? "", "캐릭터", 0.3f, 0.22f,
            TMP_InputField.ContentType.Standard, false,
            (val) =>
            {
                character = val;
                if (CharacterManager.instance != null && !string.IsNullOrEmpty(val))
                    characterData = CharacterManager.instance.GetCharacter(val);
                SyncToTsvCommand();
            });

        // 표정 (클릭→순환)
        expressionLabel = CreateEnumSelector(container, "lbl_exp",
            new Color(1f, 0.85f, 0.3f), 0.22f,
            () => { CycleEnum(ref expression); RefreshNodeUI(); SyncToTsvCommand(); });

        // 도착 위치 (클릭→순환)
        endLocLabel = CreateEnumSelector(container, "lbl_to",
            new Color(0.5f, 0.8f, 1f), 0.22f,
            () => { CycleEnum(ref endLocation); RefreshNodeUI(); SyncToTsvCommand(); });

        // 이징 (클릭→순환)
        easeLabel = CreateEnumSelector(container, "lbl_ease",
            new Color(1f, 0.6f, 0.6f), 0.22f,
            () => { CycleEaseType(); RefreshNodeUI(); SyncToTsvCommand(); });

        // 시간 (직접 입력)
        durationInput = CreateTMPInputField(container, "input_dur",
            wait.ToString("F1"), "시간(초)", 0.3f, 0.22f,
            TMP_InputField.ContentType.DecimalNumber, false,
            (val) =>
            {
                if (float.TryParse(val, out float f))
                    wait = f;
                SyncToTsvCommand();
            });
    }

    // ── UI 업데이트 ─────────────────────────────────────

    protected override void RefreshNodeUI()
    {
        if (expressionLabel == null) return;

        if (characterInput != null)
        {
            string charName = character;
            if (string.IsNullOrEmpty(charName))
                charName = tsvCommand?.name ?? "";
            characterInput.SetTextWithoutNotify(charName);
        }

        expressionLabel.text = $"Exp: {expression}";
        endLocLabel.text = $"Pos: {endLocation}";
        easeLabel.text = $"Ease: {easeType}";

        if (durationInput != null)
            durationInput.SetTextWithoutNotify(wait.ToString("F1"));
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
