using System.Collections.Generic;
using UnityEngine;

public sealed class StoryDatabase : MonoBehaviour
{
    public static StoryDatabase instance;

    public TextAsset tsvAsset;

    public List<TsvCommand> Commands = new();
    public Dictionary<int, TsvCommand> ById { get; private set; } = new();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (tsvAsset == null)
        {
            Debug.LogWarning("[StoryDatabase] tsvAsset이 할당되지 않았습니다.");
            return;
        }

        LoadAsset(tsvAsset);
    }

    public void LoadAsset(TextAsset asset)
    {
        Commands = TsvParser.Parse(asset);
        ById.Clear();
        foreach (var cmd in Commands)
        {
            if (!ById.ContainsKey(cmd.Id))
                ById.Add(cmd.Id, cmd);
            else
                Debug.LogWarning($"[StoryDatabase] Duplicate Id: {cmd.Id}");
        }
    }

    public bool TryGet(int id, out TsvCommand cmd) => ById.TryGetValue(id, out cmd);
}
