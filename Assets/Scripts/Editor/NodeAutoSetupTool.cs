using TMPro;
using UnityEditor;
using UnityEngine;

public static class NodeAutoSetupTool
{
    private const string FontPath = "Assets/TextMesh Pro/Fonts/Maplestory Light SDF.asset";

    [MenuItem("Tools/Node Graph/Run AutoSetup on All Node Prefabs")]
    public static void RunAutoSetupOnAllPrefabs()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
            Debug.LogWarning($"[AutoSetup] 폰트를 찾지 못했습니다: {FontPath}");

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int count = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var node = scope.prefabContentsRoot.GetComponent<BaseNode>();
                if (node == null) continue;

                node.AutoSetup();
                node.ApplyNodeSize();

                if (font != null)
                    node.nodeFont = font;

                count++;
                Debug.Log($"[AutoSetup] {path}  |  size={node.nodeSize}  idTmp={node.idTmp}  statTmp={node.statTmp}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[AutoSetup] 완료 — {count}개 노드 프리팹 처리됨");
    }
}
