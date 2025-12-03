using System.Collections.Generic;
using UnityEngine;



public abstract class BasePanel : MonoBehaviour
{
   
    public List<string> objNames = new List<string>();
    // 자식 UI 루트들
    protected readonly List<GameObject> uis = new List<GameObject>();
    // 이름 → GameObject 맵
    protected readonly Dictionary<string, GameObject> uiMap = new Dictionary<string, GameObject>();

    protected virtual void Awake()
    {
        CacheChildUis();
    }

    /// <summary>
    /// 직계 자식들 중에서 MonoBehaviour 달린 오브젝트를 UI로 등록
    /// </summary>
    void CacheChildUis()
    {
        uiMap.Clear();
        uis.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var go = child.gameObject;

            // MonoBehaviour가 하나라도 붙어있으면 "UI 루트"라고 가정
            if (!go.TryGetComponent<MonoBehaviour>(out _))
                continue;

            // 이름 중복 안전 장치
            if (uiMap.ContainsKey(go.name))
            {
                Debug.LogWarning($"[Base_Panel] 같은 이름의 UI가 중복됨: {go.name}", this);
                continue;
            }

            uis.Add(go);
            uiMap.Add(go.name, go);
        }
    }

    /// <summary>
    /// 이름으로 UI 컴포넌트 가져오기 (없으면 null 리턴)
    /// </summary>
    public T GetUi<T>(string name) where T : Component
    {
        if (!uiMap.TryGetValue(name, out var go))
        {
            Debug.LogError($"[Base_Panel] UI 이름을 찾을 수 없음: {name}", this);
            return null;
        }

        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            Debug.LogError($"[Base_Panel] {name} 오브젝트에 {typeof(T).Name} 컴포넌트가 없음", this);
        }
        return comp;
    }

    /// <summary>
    /// Try 패턴도 필요하면
    /// </summary>
    public bool TryGetUi<T>(string name, out T comp) where T : Component
    {
        comp = null;

        if (!uiMap.TryGetValue(name, out var go))
            return false;

        comp = go.GetComponent<T>();
        return comp != null;
    }
}
