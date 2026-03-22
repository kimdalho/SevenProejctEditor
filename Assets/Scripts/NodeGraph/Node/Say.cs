using System.Collections;
using UnityEngine;

public class Say : BaseNode
{
    public override IEnumerator Execute(NodePlayer player)
    {
        var ui = player.dialogueUI;
        yield return ui.ShowDialogue(character, str_1);
        yield return ui.WaitForInput();
    }
}
