using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Wait : BaseNode
{
    private TMP_InputField waitInput;

    protected override void BuildNodeUI()
    {
        var container = GetOrCreateContentContainer();
        if (container == null) return;

        waitInput = CreateTMPInputField(container, "input_wait",
            wait.ToString("F1"), "대기 시간(초)", 0.3f, 0.22f,
            TMP_InputField.ContentType.DecimalNumber, false,
            (val) =>
            {
                if (float.TryParse(val, out float f))
                    wait = f;
                SyncToTsvCommand();
            });
    }

    protected override void RefreshNodeUI()
    {
        if (waitInput != null)
            waitInput.SetTextWithoutNotify(wait.ToString("F1"));
    }

    private void SyncToTsvCommand()
    {
        if (tsvCommand == null) return;
        if (tsvCommand.Fields == null)
            tsvCommand.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        tsvCommand.Fields["wait"] = wait.ToString("F1");
    }

    public override IEnumerator Execute(NodePlayer player)
    {
        yield return new WaitForSeconds(wait);
    }
}
