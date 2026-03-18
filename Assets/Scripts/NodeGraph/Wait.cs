using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Wait : BaseNode
{
    public InputField InputField;

    public override void SetCommnadData(TsvCommand cmd)
    {
        base.SetCommnadData(cmd);
        InputField.text = wait.ToString();
    }

    public override IEnumerator Execute(NodePlayer player)
    {
        yield return new WaitForSeconds(wait);
    }
}
