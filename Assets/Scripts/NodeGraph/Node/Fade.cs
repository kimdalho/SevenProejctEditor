using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Fade : BaseNode
{
    public EaseType easeType = EaseType.Linear;

    // 에디터 UI (코드로 자동 생성)
    private TextMeshProUGUI dirLabel;
    private TextMeshProUGUI easeLabel;
    private TextMeshProUGUI durationLabel;
    private bool uiBuilt;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);

        // TSV에서 duration 읽기 (없으면 wait 사용)
        if (cmd.Has("duration"))
            wait = cmd.GetFloat("duration", 1f);

        // 이징 타입 읽기
        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out easeType);

        // UI 빌드 (최초 1회)
        if (!uiBuilt)
            BuildFadeUI();

        RefreshLabels();
    }

    // ── 에디터 UI 생성 ─────────────────────────────────

    private void BuildFadeUI()
    {
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) return;

        // 기존 콘텐츠 아래에 파라미터 패널 생성
        var containerObj = new GameObject("FadeParams");
        containerObj.transform.SetParent(canvas.transform, false);

        var containerRT = containerObj.AddComponent<RectTransform>();
        containerRT.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        containerRT.anchoredPosition = new Vector2(0f, -0.055f);
        containerRT.sizeDelta = new Vector2(0.9f, 1.0f);

        var layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.spacing = -0.02f;
        layout.childAlignment = TextAnchor.UpperCenter;

        // 방향 (클릭→FadeOut/FadeIn 토글)
        dirLabel = CreateParamLabel(containerObj.transform, "lbl_dir",
            new Color(1f, 0.6f, 0.6f), 0.25f,
            () => { ToggleDirection(); RefreshLabels(); SyncToTsvCommand(); });

        // 이징 타입 (클릭→6가지 순환)
        easeLabel = CreateParamLabel(containerObj.transform, "lbl_ease",
            new Color(1f, 0.85f, 0.3f), 0.22f,
            () => { CycleEaseType(); RefreshLabels(); SyncToTsvCommand(); });

        // 시간 (클릭→프리셋 순환)
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

        int dir = GetDirection();
        dirLabel.text = dir == 2 ? "FadeIn" : "FadeOut";
        easeLabel.text = $"Ease: {easeType}";
        durationLabel.text = $"Dur: {wait:F1}s";
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

        tsvCommand.Fields["easeType"] = easeType.ToString();
        tsvCommand.Fields["wait"] = wait.ToString("F1");
        tsvCommand.Fields["duration"] = wait.ToString("F1");
    }

    // ── 런타임 실행 ─────────────────────────────────────

    public override IEnumerator Execute(NodePlayer player)
    {
        var fadeUI = player.fadeUI;

        // StateInt로 방향 결정: 1=FadeOut(어두워짐), 2=FadeIn(밝아짐)
        int dir = tsvCommand != null && tsvCommand.StateInt.HasValue
            ? tsvCommand.StateInt.Value
            : 1;

        float duration = wait > 0f ? wait : 1f;
        yield return fadeUI.DoFade(dir, duration, easeType);
    }
}
