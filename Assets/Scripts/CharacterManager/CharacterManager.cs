using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);       
    }
    [Header("Character")]
    public List<CharacterData> characterDatas;
    public Dictionary<(string characterName, eExpression expression), CharacterData> characterMap = new();


}
