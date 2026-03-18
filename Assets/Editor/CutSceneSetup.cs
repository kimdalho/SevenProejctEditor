using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메뉴: Tools > CutScene > Create CutScene Scene
/// CutScene 씬을 자동 생성하고 Build Settings에 등록한다.
/// </summary>
public static class CutSceneSetup
{
    [MenuItem("Tools/CutScene/Create CutScene Scene")]
    public static void CreateCutSceneScene()
    {
        string scenePath = "Assets/Scenes/CutScene.unity";

        // 폴더 확인
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        // 새 씬 생성
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // CutScenePlayer 오브젝트 배치
        var playerObj = new GameObject("CutScenePlayer");
        playerObj.AddComponent<CutScenePlayer>();

        // 씬 저장
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[CutSceneSetup] 씬 생성: {scenePath}");

        // Build Settings에 등록
        AddSceneToBuildSettings(scenePath);

        // 원래 씬으로 복귀할 수 있도록 현재 에디터 씬도 Build Settings에 등록
        EnsureActiveSceneInBuild();

        AssetDatabase.Refresh();
        Debug.Log("[CutSceneSetup] 완료! Build Settings에 CutScene 씬이 등록되었습니다.");
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);

        foreach (var s in scenes)
        {
            if (s.path == scenePath)
            {
                s.enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[CutSceneSetup] Build Settings에 추가: {scenePath}");
    }

    private static void EnsureActiveSceneInBuild()
    {
        // 에디터에서 마지막으로 열었던 씬(에디터 씬)도 Build Settings에 등록
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            bool found = false;
            foreach (var s in scenes)
            {
                if (s.path == path) { found = true; break; }
            }
            if (!found)
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"[CutSceneSetup] Build Settings에 추가: {path}");
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
