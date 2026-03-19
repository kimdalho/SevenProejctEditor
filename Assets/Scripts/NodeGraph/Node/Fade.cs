using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Fade : BaseNode
{
    public EaseType easeType = EaseType.Linear;

    // 에디터 UI
    private TextMeshProUGUI dirLabel;
    private TextMeshProUGUI easeLabel;
    private TMP_InputField durationInput;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);

        // Fade 전용 파싱
        if (cmd.Has("duration"))
            wait = cmd.GetFloat("duration", 1f);

        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out easeType);

        RefreshNodeUI();
    }

    // ── 에디터 UI 생성 ─────────────────────────────────

    protected override void BuildNodeUI()
    {
        var container = GetOrCreateContentContainer();
        if (container == null) return;

        // 방향 (클릭→FadeOut/FadeIn 토글)
        dirLabel = CreateEnumSelector(container, "lbl_dir",
            new Color(1f, 0.6f, 0.6f), 0.25f,
            () => { ToggleDirection(); RefreshNodeUI(); SyncToTsvCommand(); });

        // 이징 타입 (클릭→순환)
        easeLabel = CreateEnumSelector(container, "lbl_ease",
            new Color(1f, 0.85f, 0.3f), 0.22f,
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
        if (dirLabel == null) return;

        int dir = GetDirection();
        dirLabel.text = dir == 2 ? "FadeIn" : "FadeOut";
        easeLabel.text = $"Ease: {easeType}";

        if (durationInput != null)
            durationInput.SetTextWithoutNotify(wait.ToString("F1"));
    }

    // ── 값 조작 ─────────────────────────────────────────

    private int GetDirection()
    {
        return tsvCommand != null && tsvCommand.StateInt.HasValue
            ? tsvCommand.StateInt.Value : 1;
    }

    private void ToggleDirection()
    {
        if (tsvCommand == null) return;
        int cur = tsvCommand.StateInt ?? 1;
        tsvCommand.StateInt = (cur == 1) ? 2 : 1;
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

        tsvCommand.Fields["easeType"] = easeType.ToString();
        tsvCommand.Fields["wait"] = wait.ToString("F1");
        tsvCommand.Fields["duration"] = wait.ToString("F1");
    }

    // ── 런타임 실행 ─────────────────────────────────────

    public override IEnumerator Execute(NodePlayer player)
    {
        var fadeUI = player.fadeUI;

        int dir = tsvCommand != null && tsvCommand.StateInt.HasValue
            ? tsvCommand.StateInt.Value
            : 1;

        float duration = wait > 0f ? wait : 1f;
        yield return fadeUI.DoFade(dir, duration, easeType);
    }
}
