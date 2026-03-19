using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePlayer : MonoBehaviour
{
    public static NodePlayer instance;

    [Header("Prefab")]
    [SerializeField] private GameObject cutsceneCanvasPrefab;

    [Header("UI References")]
    public DialogueUI dialogueUI;
    public FadeUI fadeUI;
    public CharacterDisplay characterDisplay;

    [Header("State")]
    public bool isPlaying;
    public BaseNode currentNode;

    private Coroutine playCoroutine;
    private Canvas runtimeCanvas;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildUI();
    }

    private void BuildUI()
    {
        if (cutsceneCanvasPrefab == null)
        {
            Debug.LogWarning("[NodePlayer] cutsceneCanvasPrefab이 할당되지 않았습니다. Inspector에서 프리팹을 연결하세요.");
            return;
        }

        var canvasInstance = Instantiate(cutsceneCanvasPrefab, transform);
        runtimeCanvas = canvasInstance.GetComponent<Canvas>();
        dialogueUI = canvasInstance.GetComponentInChildren<DialogueUI>(true);
        fadeUI = canvasInstance.GetComponentInChildren<FadeUI>(true);
        characterDisplay = canvasInstance.GetComponentInChildren<CharacterDisplay>(true);

        // 초기 상태
        dialogueUI.Hide();
        fadeUI.SetClear();
        characterDisplay.HideAll();

        runtimeCanvas.gameObject.SetActive(false);
    }

    public void Play()
    {
        var nodes = NodeGraphManager.instance.nodes;
        if (nodes == null || nodes.Count == 0)
        {
            Debug.LogWarning("[NodePlayer] 재생할 노드가 없습니다.");
            return;
        }

        // 첫 번째 노드 = 체인의 시작 (PrevNode가 없는 노드)
        BaseNode startNode = null;
        foreach (var node in nodes)
        {
            if (node.PrevNode == null)
            {
                startNode = node;
                break;
            }
        }

        if (startNode == null)
            startNode = nodes[0];

        Play(startNode);
    }

    public void Play(BaseNode startNode)
    {
        if (isPlaying) return;

        isPlaying = true;
        runtimeCanvas.gameObject.SetActive(true);
        playCoroutine = StartCoroutine(PlayCoroutine(startNode));
    }

    public void Stop()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        isPlaying = false;
        currentNode = null;

        dialogueUI.Hide();
        fadeUI.SetClear();
        characterDisplay.HideAll();
        runtimeCanvas.gameObject.SetActive(false);
    }

    private IEnumerator PlayCoroutine(BaseNode startNode)
    {
        currentNode = startNode;

        while (currentNode != null)
        {
            yield return currentNode.Execute(this);
            currentNode = currentNode.NextNode;
        }

        Stop();
    }
}
