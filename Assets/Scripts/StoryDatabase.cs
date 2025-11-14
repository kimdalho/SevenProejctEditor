using System.Collections.Generic;
using UnityEngine;

public sealed class StoryDatabase : MonoBehaviour
{
    public static StoryDatabase instance;

    [Tooltip("Resources 경로(확장자 제외). 예: 'Story/episode1'")]
    public string resourcesPath = "Story/episode1";

    public List<TsvCommand> Commands = new();
    public Dictionary<int, TsvCommand> ById { get; private set; } = new();

    void Awake()
    {
        //1) 싱글톤
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


        //2) 리소스 확인
        var ta = Resources.Load<TextAsset>(resourcesPath);
        if (ta == null)
        {
            Debug.LogError($"[StoryDatabase] TSV not found at Resources/{resourcesPath}");
            return;
        }

        //3) tsv 데이터 파싱
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
