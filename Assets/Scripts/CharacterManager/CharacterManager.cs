using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터 데이터 관리. Resources/Characters/ 폴더에서 자동 로드.
///
/// 폴더 구조:
///   Resources/Characters/{이름}.png          → 기본 스프라이트
///   Resources/Characters/{이름}_happy.png    → Happy 표정
///   Resources/Characters/{이름}_sad.png      → Sad 표정
///   Resources/Characters/{이름}_angry.png    → Angry 표정
///   Resources/Characters/{이름}_surprise.png → Surprise 표정
/// </summary>
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;

    [Header("Character")]
    public string resourcesPath = "Characters";
    public List<CharacterData> characterDatas = new();

    private Dictionary<string, CharacterData> characterByName = new();

    // 하위 호환용
    public Dictionary<(string characterName, eExpression expression), CharacterData> characterMap = new();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadFromResources();
    }

    private void LoadFromResources()
    {
        // Resources/Characters/ 아래 모든 스프라이트 로드
        var sprites = Resources.LoadAll<Sprite>(resourcesPath);
        if (sprites.Length == 0)
        {
            Debug.LogWarning($"[CharacterManager] Resources/{resourcesPath}/ 에 스프라이트가 없습니다.");
            return;
        }

        // 이름별로 분류
        // 파일명 규칙: {캐릭터이름} → 기본, {캐릭터이름}_{표정} → 표정 스프라이트
        var spriteMap = new Dictionary<string, Dictionary<string, Sprite>>();

        foreach (var sprite in sprites)
        {
            string fullName = sprite.name.ToLower();
            string charName;
            string expName = "";

            int underscoreIdx = fullName.LastIndexOf('_');
            if (underscoreIdx > 0)
            {
                string suffix = fullName.Substring(underscoreIdx + 1);
                // 표정 이름인지 확인
                if (IsExpressionName(suffix))
                {
                    charName = fullName.Substring(0, underscoreIdx);
                    expName = suffix;
                }
                else
                {
                    charName = fullName;
                }
            }
            else
            {
                charName = fullName;
            }

            if (!spriteMap.ContainsKey(charName))
                spriteMap[charName] = new Dictionary<string, Sprite>();

            spriteMap[charName][expName] = sprite;
        }

        // CharacterData 생성
        foreach (var kvp in spriteMap)
        {
            var data = new CharacterData { name = kvp.Key };

            if (kvp.Value.TryGetValue("", out var defaultSpr))
                data.defaultSprite = defaultSpr;

            foreach (var expKvp in kvp.Value)
            {
                if (string.IsNullOrEmpty(expKvp.Key)) continue;
                if (TryParseExpression(expKvp.Key, out var exp))
                {
                    data.expressions.Add(new Ex
                    {
                        sprite = expKvp.Value,
                        expression = exp
                    });
                }
            }

            // 기본 스프라이트가 없으면 첫 번째 스프라이트 사용
            if (data.defaultSprite == null && kvp.Value.Count > 0)
            {
                foreach (var v in kvp.Value.Values) { data.defaultSprite = v; break; }
            }

            characterDatas.Add(data);
            characterByName[data.name] = data;

            // 하위 호환 딕셔너리
            characterMap[(data.name, eExpression.None)] = data;
            foreach (var ex in data.expressions)
                characterMap[(data.name, ex.expression)] = data;
        }

        Debug.Log($"[CharacterManager] {characterDatas.Count}명 로드 완료 (from Resources/{resourcesPath}/)");
    }

    /// <summary>
    /// 이름으로 캐릭터 데이터 가져오기
    /// </summary>
    public CharacterData GetCharacter(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        characterByName.TryGetValue(name.ToLower(), out var data);
        return data;
    }

    private static bool IsExpressionName(string s)
    {
        return s == "none" || s == "happy" || s == "sad" || s == "angry" || s == "surprise";
    }

    private static bool TryParseExpression(string s, out eExpression exp)
    {
        return System.Enum.TryParse(s, true, out exp);
    }
}
