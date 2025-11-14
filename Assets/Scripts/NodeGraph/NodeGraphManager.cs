using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

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

        //데이터 노드화
        if(Input.GetKeyDown(KeyCode.D))
        {
            CreateNodeToTsvCommand();
        }

    }

    //----------------- 노드 생성 ------------------------------------------

    public void CreateNodeToType(eNodeType type,BaseNode prefab)
    {
        var obj = Instantiate(prefab);
        var newbaseNode = obj.GetComponent<BaseNode>();
        newbaseNode.transform.position = Vector3.zero;
        nodes.Add(newbaseNode);
    }

    public Vector3 startPos = new Vector3(-3.5f,0,2.5f);
    public float offset = 0.5f;
    public void CreateNodeToTsvCommand()
    {
        //-3.5,0,2.5 -> -2,0,2.5
        //1) 노드 생성
        var data = StoryDatabase.instance.Commands;
        int curY = -1;
        for (int i = 0; i < data.Count; i++)
        {
            int curX = i % 20;

            if (curX % 20 == 0)
            {
                curY++;
            }
            BaseNode node = null;
            switch(data[i].StateStr)
            {
                case "say":
                    node = Instantiate(NodePrefab1);
                    break;
                default:
                    node = Instantiate(NodePrefab);
                    break;
            }

           

            node.SetCommnadData(data[i]);
            node.transform.position = new Vector3(-3.5f + (curX * offset), 0, 2.5f - (curY * offset));
            nodes.Add(node);
        }

        //2) 노드 자동 링크

        for (int i = 0; i < nodes.Count -1; i++)
        {
            if (nodes[i].id <= -1) return;
            nodes[i].nextButton.gameObject.name = $"시작_{i}";
            nodes[i].prevButton.gameObject.name = $"이전_{i}";


            nodes[i].SetLinkNext(nodes[i+1]);
        }
    }

}
