using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TsvParser
{
    // TSV �� �� �и� (�� ����, �⺻���� �̽��������� ����)
    // �ʿ��ϸ� ���ߵ���ǥ ó��/�̽������� ��ȭ ����.
    private static List<string> SplitTsvLine(string line)
    {
        // ���� �ܼ�/�������� ���: '\t' �ܼ� �и� �� ������ ��ĭ �е��� ȣ��ο��� ó��
        return new List<string>(line.Split('\t'));
    }

    public static List<TsvCommand> Parse(TextAsset textAsset)
    {
        if (textAsset == null) throw new ArgumentNullException(nameof(textAsset));
        return Parse(textAsset.text);
    }

    public static List<TsvCommand> Parse(string tsvText)
    {
        // BOM/���� ����
        if (string.IsNullOrEmpty(tsvText)) return new List<TsvCommand>();
        tsvText = tsvText.Replace("\r\n", "\n").Replace("\r", "\n");

        using var sr = new StringReader(tsvText);
        string headerLine = sr.ReadLine();
        if (headerLine == null) return new List<TsvCommand>();

        // �ּ�/���� ��ŵ�ذ��� ��� ã��
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

            // �÷� ���� �����ϸ� ���ڿ��� �е�
            while (cols.Count < headers.Count) cols.Add(string.Empty);

            // ����
            var record = new TsvCommand();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Count; i++)
            {
                var key = headers[i]?.Trim();
                if (string.IsNullOrEmpty(key)) continue;
                var val = i < cols.Count ? cols[i] : string.Empty;

                // �ھ� �÷� ó��
                if (key.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(val, out record.Id))
                    {
                        Debug.LogWarning($"[TSV] Invalid Id at line {lineNo}: '{val}' �� default 0");
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

                // Ȯ�� �÷��� ���� Fields��
                dict[key] = val;
            }

            // StateStr가 비어있으면 유효한 커맨드가 아니므로 건너뜀
            if (string.IsNullOrWhiteSpace(record.StateStr)) continue;

            record.Fields = dict;

            results.Add(record);
        }

        return results;
    }
}
