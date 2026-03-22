using System.Collections;
using UnityEngine;

public class Wait : BaseNode
{
    public override IEnumerator Execute(NodePlayer player)
    {
        yield return new WaitForSeconds(wait);
    }
}
