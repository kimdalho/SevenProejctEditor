using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TsvParser
{
    // TSV 한 줄 분리 (탭 기준, 기본적인 이스케이프만 지원)
    // 필요하면 이중따옴표 처리/이스케이프 강화 가능.
    private static List<string> SplitTsvLine(string line)
    {
        // 가장 단순/안정적인 방법: '\t' 단순 분리 후 부족분 빈칸 패딩은 호출부에서 처리
        return new List<string>(line.Split('\t'));
    }

    public static List<TsvCommand> Parse(TextAsset textAsset)
    {
        if (textAsset == null) throw new ArgumentNullException(nameof(textAsset));
        return Parse(textAsset.text);
    }

    public static List<TsvCommand> Parse(string tsvText)
    {
        // BOM/개행 정리
        if (string.IsNullOrEmpty(tsvText)) return new List<TsvCommand>();
        tsvText = tsvText.Replace("\r\n", "\n").Replace("\r", "\n");

        using var sr = new StringReader(tsvText);
        string headerLine = sr.ReadLine();
        if (headerLine == null) return new List<TsvCommand>();

        // 주석/빈줄 스킵해가며 헤더 찾기
        while (headerLine != null && (headerLine.Length == 0 || headerLine.StartsWith("#")))
            headerLine = sr.ReadLine();

        if (headerLine == null) return new List<TsvCommand>();

        var headers = SplitTsvLine(headerLine);
        var results = new List<TsvCommand>();

        string line;
        int lineNo = 1; // header = line1
        while ((line = sr.ReadLine()) != null)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            var cols = SplitTsvLine(line);

            // 컬럼 개수 부족하면 빈문자열로 패딩
            while (cols.Count < headers.Count) cols.Add(string.Empty);

            // 맵핑
            var record = new TsvCommand();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Count; i++)
            {
                var key = headers[i]?.Trim();
                if (string.IsNullOrEmpty(key)) continue;
                var val = i < cols.Count ? cols[i] : string.Empty;

                // 코어 컬럼 처리
                if (key.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(val, out record.Id))
                    {
                        Debug.LogWarning($"[TSV] Invalid Id at line {lineNo}: '{val}' → default 0");
                        record.Id = 0;
                    }
                    continue;
                }
                if (key.Equals("state_str", StringComparison.OrdinalIgnoreCase))
                {
                    record.StateStr = val?.Trim();
                    continue;
                }
                if (key.Equals("state_int", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(val, out var si)) record.StateInt = si;
                    else record.StateInt = null;
                    continue;
                }

                // 확장 컬럼은 전부 Fields로
                dict[key] = val;
            }

            // 누락 방어
            record.StateStr ??= string.Empty;
            record.Fields = dict;

            results.Add(record);
        }

        return results;
    }
}
