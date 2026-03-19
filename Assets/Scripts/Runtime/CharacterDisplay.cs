using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplay : MonoBehaviour
{
    private readonly Dictionary<string, Image> activeCharacters = new();
    [SerializeField] private RectTransform layerRect;

    // 화면 내 3포지션 (X 비율 0~1)
    private static readonly Dictionary<eLocation, float> positionMap = new()
    {
        { eLocation.Left,  0.2f },
        { eLocation.Mid,   0.5f },
        { eLocation.Right, 0.8f },
    };

    // 화면 밖 시작 위치
    private const float OUT_LEFT = -0.3f;
    private const float OUT_RIGHT = 1.3f;

    private void Awake()
    {
        // layerRect가 Inspector에서 연결되지 않았으면 자기 자신의 RectTransform 사용
        if (layerRect == null)
            layerRect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 캐릭터를 화면 밖에서 목표 위치로 이동하며 등장.
    /// 시작 위치는 목표 위치에 따라 자동 결정:
    ///   Left  → 왼쪽 밖에서 등장
    ///   Right → 오른쪽 밖에서 등장
    ///   Mid   → 왼쪽 밖에서 등장
    /// </summary>
    public IEnumerator ShowCharacter(CharacterData data, eExpression expression,
        eLocation targetLoc, float duration = 0.5f, EaseType ease = EaseType.OutCubic)
    {
        if (data == null || string.IsNullOrEmpty(data.name))
        {
            Debug.LogWarning("[CharacterDisplay] CharacterData가 null이거나 이름이 비어있습니다.");
            yield break;
        }

        // 스프라이트 결정
        Sprite sprite = data.GetSprite(expression);

        // 캐릭터 Image 가져오거나 생성
        bool isNew = false;
        if (!activeCharacters.TryGetValue(data.name, out var charImg) || charImg == null)
        {
            charImg = CreateCharacterImage(data.name);
            activeCharacters[data.name] = charImg;
            isNew = true;
        }

        // 스프라이트 적용
        if (sprite != null)
        {
            charImg.sprite = sprite;
            charImg.color = Color.white;
        }
        else
        {
            charImg.sprite = null;
            charImg.color = new Color(0.3f, 0.3f, 0.4f, 0.7f);
        }

        charImg.gameObject.SetActive(true);

        // 목표 위치
        float endX = GetPositionX(targetLoc);

        // 시작 위치: 화면 밖에서 (목표에 따라 자동 결정)
        float startX;
        if (isNew)
        {
            startX = (targetLoc == eLocation.Right) ? OUT_RIGHT : OUT_LEFT;
        }
        else
        {
            // 이미 화면에 있는 캐릭터: 현재 위치에서 이동
            var rt = charImg.rectTransform;
            float screenWidth = layerRect.rect.width;
            startX = (screenWidth > 0f) ? (rt.anchoredPosition.x / screenWidth + 0.5f) : endX;
        }

        // 애니메이션
        yield return AnimateMove(charImg, startX, endX, duration, ease, isNew);
    }

    private IEnumerator AnimateMove(Image charImg, float startX, float endX,
        float duration, EaseType ease, bool fadeIn)
    {
        var rt = charImg.rectTransform;
        float screenWidth = layerRect.rect.width;
        duration = Mathf.Max(duration, 0.01f);
        float elapsed = 0f;

        // 시작 상태
        rt.anchoredPosition = new Vector2((startX - 0.5f) * screenWidth, 0f);
        if (fadeIn) SetAlpha(charImg, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Easing.Evaluate(ease, t);

            // 이동
            float x = Mathf.Lerp(startX, endX, easedT);
            rt.anchoredPosition = new Vector2((x - 0.5f) * screenWidth, 0f);

            // 페이드인 (새 캐릭터)
            if (fadeIn)
                SetAlpha(charImg, Mathf.Lerp(0f, 1f, easedT));

            yield return null;
        }

        // 최종 상태
        rt.anchoredPosition = new Vector2((endX - 0.5f) * screenWidth, 0f);
        if (fadeIn) SetAlpha(charImg, 1f);
    }

    /// <summary>
    /// 특정 캐릭터의 표정만 변경
    /// </summary>
    public void ChangeExpression(string characterName, CharacterData data, eExpression expression)
    {
        if (!activeCharacters.TryGetValue(characterName, out var charImg)) return;
        if (data == null) return;

        Sprite sprite = data.GetSprite(expression);
        if (sprite != null)
            charImg.sprite = sprite;
    }

    /// <summary>
    /// 특정 캐릭터 페이드아웃 후 제거
    /// </summary>
    public IEnumerator HideCharacter(string characterName, float duration = 0.3f)
    {
        if (!activeCharacters.TryGetValue(characterName, out var charImg)) yield break;

        float elapsed = 0f;
        duration = Mathf.Max(duration, 0.01f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(charImg, 1f - t);
            yield return null;
        }

        Destroy(charImg.gameObject);
        activeCharacters.Remove(characterName);
    }

    /// <summary>
    /// 모든 캐릭터 즉시 제거
    /// </summary>
    public void HideAll()
    {
        foreach (var kvp in activeCharacters)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        activeCharacters.Clear();
    }

    private Image CreateCharacterImage(string charName)
    {
        var charObj = new GameObject($"Char_{charName}");
        charObj.transform.SetParent(layerRect, false);

        var rt = charObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0.85f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(500f, 0f);
        rt.anchoredPosition = Vector2.zero;

        var img = charObj.AddComponent<Image>();
        img.preserveAspect = true;
        img.raycastTarget = false;

        return img;
    }

    private float GetPositionX(eLocation loc)
    {
        return positionMap.TryGetValue(loc, out var x) ? x : 0.5f;
    }

    private void SetAlpha(Image img, float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}
