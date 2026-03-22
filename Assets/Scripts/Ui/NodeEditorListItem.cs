using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NodeEditorPanel 스크롤 리스트의 한 행(row).
/// </summary>
public class NodeEditorListItem : MonoBehaviour
{
    [Header("공통")]
    public TextMeshProUGUI labelText;

    [Header("입력 섹션 (텍스트/숫자)")]
    public GameObject     inputSection;
    public TMP_InputField inputField;

    [Header("드롭다운 섹션 (Enum)")]
    public GameObject    enumSection;
    public TMP_Dropdown  dropdown;

    [Header("읽기 전용 섹션")]
    public GameObject      readOnlySection;
    public TextMeshProUGUI readOnlyLabel;

    // ── 설정 메서드 ────────────────────────────────────────

    public void SetupInput(string label, string value, bool multiline, Action<string> onChange)
    {
        labelText.text = label;

        inputSection.SetActive(true);
        enumSection.SetActive(false);
        readOnlySection.SetActive(false);

        var le = GetComponent<LayoutElement>();
        if (le != null) le.preferredHeight = multiline ? 88f : 52f;

        inputField.lineType = multiline
            ? TMP_InputField.LineType.MultiLineNewline
            : TMP_InputField.LineType.SingleLine;

        inputField.SetTextWithoutNotify(value ?? "");
        inputField.onValueChanged.RemoveAllListeners();
        if (onChange != null)
            inputField.onValueChanged.AddListener(v => onChange(v));
    }

    public void SetupEnum(string label, string[] options, int currentIndex, Action<int> onChange)
    {
        labelText.text = label;

        inputSection.SetActive(false);
        enumSection.SetActive(true);
        readOnlySection.SetActive(false);

        var le = GetComponent<LayoutElement>();
        if (le != null) le.preferredHeight = 52f;

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        dropdown.SetValueWithoutNotify(currentIndex);
        dropdown.onValueChanged.RemoveAllListeners();
        if (onChange != null)
            dropdown.onValueChanged.AddListener(idx => onChange(idx));
    }

    public void SetupReadOnly(string label, string value)
    {
        labelText.text = label;

        inputSection.SetActive(false);
        enumSection.SetActive(false);
        readOnlySection.SetActive(true);

        var le = GetComponent<LayoutElement>();
        if (le != null) le.preferredHeight = 36f;

        readOnlyLabel.text = value;
    }

    // ── 런타임 갱신 ────────────────────────────────────────

    public void UpdateReadOnly(string value)
    {
        if (readOnlyLabel != null) readOnlyLabel.text = value;
    }
}
