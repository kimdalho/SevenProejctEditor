using System;
using System.Collections.Generic;

[Serializable]
public sealed class TsvCommand
{
    public int Id;                    // 필수
    public string StateStr;           // 예: "say", "fade"
    public int? StateInt;             // 선택 (없으면 null)

    // 나머지 모든 컬럼(예: str_1, str_2, speaker, dur, ease, type, ...)
    public Dictionary<string, string> Fields = new();

    // 값 접근 헬퍼
    public string Get(string key, string defaultValue = "")
        => Fields != null && Fields.TryGetValue(key, out var v) ? v : defaultValue;

    public int GetInt(string key, int defaultValue = 0)
        => Fields != null && int.TryParse(Get(key), out var v) ? v : defaultValue;

    public float GetFloat(string key, float defaultValue = 0f)
        => Fields != null && float.TryParse(Get(key), out var v) ? v : defaultValue;

    public bool Has(string key) => Fields != null && Fields.ContainsKey(key);
}
