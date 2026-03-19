using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Say : BaseNode
{
    private TMP_InputField characterInput;
    private TMP_InputField str1Input;

    protected override void BuildNodeUI()
    {
        var container = GetOrCreateContentContainer();
        if (container == null) return;

        characterInput = CreateTMPInputField(container, "input_char",
            character ?? "", "캐릭터", 0.3f, 0.22f,
            TMP_InputField.ContentType.Standard, false,
            (val) => { character = val; SyncToTsvCommand(); });

        str1Input = CreateTMPInputField(container, "input_str1",
            str_1 ?? "", "대사 내용...", 0.6f, 0.2f,
            TMP_InputField.ContentType.Standard, true,
            (val) => { str_1 = val; SyncToTsvCommand(); });
    }

    protected override void RefreshNodeUI()
    {
        if (characterInput != null)
            characterInput.SetTextWithoutNotify(character ?? "");
        if (str1Input != null)
            str1Input.SetTextWithoutNotify(str_1 ?? "");
    }

    private void SyncToTsvCommand()
    {
        if (tsvCommand == null) return;
        if (tsvCommand.Fields == null)
            tsvCommand.Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        tsvCommand.Fields["character"] = character ?? "";
        tsvCommand.Fields["str_1"] = str_1 ?? "";
    }

    public override IEnumerator Execute(NodePlayer player)
    {
        var ui = player.dialogueUI;
        yield return ui.ShowDialogue(character, str_1);
        yield return ui.WaitForInput();
    }
}
