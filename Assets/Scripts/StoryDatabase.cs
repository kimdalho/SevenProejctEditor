using System.Collections.Generic;
using UnityEngine;

public sealed class StoryDatabase : MonoBehaviour
{
    [Tooltip("Resources 경로(확장자 제외). 예: 'Story/episode1'")]
    public string resourcesPath = "Story/episode1";

    public List<TsvCommand> Commands = new();
    public Dictionary<int, TsvCommand> ById { get; private set; } = new();

    void Awake()
    {
        var ta = Resources.Load<TextAsset>(resourcesPath);
        if (ta == null)
        {
            Debug.LogError($"[StoryDatabase] TSV not found at Resources/{resourcesPath}");
            return;
        }

        Commands = TsvParser.Parse(ta);
        ById.Clear();
        foreach (var cmd in Commands)
        {
            if (!ById.ContainsKey(cmd.Id))
                ById.Add(cmd.Id, cmd);
            else
                Debug.LogWarning($"[StoryDatabase] Duplicate Id: {cmd.Id}");
        }

        foreach(var cmd in Commands)
        {
            Debug.Log($"{cmd.Get("character")} {cmd.Get("str_1")}");
        }

    }

    public bool TryGet(int id, out TsvCommand cmd) => ById.TryGetValue(id, out cmd);
}
