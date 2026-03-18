using UnityEngine;


[System.Serializable]
public class VisualNovelData
{
    [Header("Data")]
    // ===== NodeData =====
    public int groupId;
    public int id;
    public string character;
    public string str_1;
    public float wait;

    public virtual void SetCommnadData(TsvCommand cmd)
    {
        character = cmd.Get("character");
        str_1 = cmd.Get("str_1");
        id = cmd.Id;

        //idTmp.text = $"ID {cmd.Id}";
        //statTmp.text = cmd.StateStr;
        //gameObject.name = $"node {cmd.Id}";
    }
}