using System;
using System.Collections;
using UnityEngine;

public enum eLocation
{
    Left,
    Mid,
    Right,
}

public enum eExpression
{
    None,
    Happy,
    Sad,
    Angry,
    Surprise,
}

[System.Serializable]
public class Ex
{
    public Sprite sprite;
    public eExpression expression;
}

[System.Serializable]
public class CharacterData
{
    public string name;
    public Sprite defaultSprite;
    public System.Collections.Generic.List<Ex> expressions = new();

    public Sprite GetSprite(eExpression exp)
    {
        if (exp != eExpression.None)
        {
            foreach (var e in expressions)
                if (e.expression == exp && e.sprite != null)
                    return e.sprite;
        }
        return defaultSprite;
    }
}

public class ShowCharacter : BaseNode
{
    public CharacterData characterData;
    public eExpression   expression;
    public eLocation     endLocation;
    public EaseType      easeType = EaseType.OutCubic;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);

        if (cmd.Has("expression"))
            Enum.TryParse(cmd.Get("expression"), true, out expression);

        if (cmd.Has("endLocation"))
            Enum.TryParse(cmd.Get("endLocation"), true, out endLocation);

        if (cmd.Has("easeType"))
            Enum.TryParse(cmd.Get("easeType"), true, out easeType);

        string charName = cmd.name;
        if (string.IsNullOrEmpty(charName))
            charName = cmd.Get("character");

        if (!string.IsNullOrEmpty(charName) && string.IsNullOrEmpty(character))
            character = charName;

        if (CharacterManager.instance != null && !string.IsNullOrEmpty(charName))
            characterData = CharacterManager.instance.GetCharacter(charName);
    }

    public override IEnumerator Execute(NodePlayer player)
    {
        var display  = player.characterDisplay;
        float duration = wait > 0f ? wait : 0.5f;
        yield return display.ShowCharacter(characterData, expression, endLocation, duration, easeType);
    }
}
