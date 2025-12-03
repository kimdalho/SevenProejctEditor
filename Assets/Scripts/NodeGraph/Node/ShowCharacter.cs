using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;

public enum eLocation
{
    None,
    OutLeft,
    OutRight,   
    Left,
    Mid,
    Right,     
}

public enum eExpression
{
    None,
    Happy,
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

    public List<CharacterData> artList;    
    
}


public class ShowCharacter : BaseNode
{
    public CharacterData characterData;
    public eExpression expression;
    public eLocation startLocation;
    public eLocation endLocation;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);
        characterData = CharacterManager.instance.characterMap[(cmd.name, expression)];
    }
}
