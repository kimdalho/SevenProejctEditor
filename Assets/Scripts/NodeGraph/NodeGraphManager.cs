using System.Collections.Generic;
using UnityEngine;

public enum eNodeType
{
    None,
    Say,
    Fade,
    CharacterShow

}

//모든 타입의 노드는 데이터테이블의 데이터를 받는데 타입영향을 받지 않아야한다.
public interface INodeDataGetService
{
    public void SetData();
}


public class NodeGraphManager : MonoBehaviour
{
    [Header("Prefabs")]

    public BaseNode NodePrefab;
    public BaseNode NodePrefab1;
    public BaseNode NodePrefab2;
    public BaseNode NodePrefab3;
    public BaseNode NodePrefab4;


    public static NodeGraphManager instance;

    public List<BaseNode> nodes = new();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            CreateNodeToType(eNodeType.None, NodePrefab);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            CreateNodeToType(eNodeType.Say, NodePrefab1);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            CreateNodeToType(eNodeType.Fade, NodePrefab2);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            CreateNodeToType(eNodeType.CharacterShow, NodePrefab3);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            CreateNodeToType(eNodeType.None, NodePrefab4);
        }
    }

    public void CreateNodeToType(eNodeType type,BaseNode prefab)
    {
        var obj = Instantiate(prefab);
        var newbaseNode = obj.GetComponent<BaseNode>();
        newbaseNode.transform.position = Vector3.zero;
        nodes.Add(newbaseNode);
    }


}
