using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : BaseNode
{
    public EaseType easeType = EaseType.Linear;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);

        if (cmd.Has("duration"))
            wait = cmd.GetFloat("duration", 1f);

        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out easeType);
    }

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
