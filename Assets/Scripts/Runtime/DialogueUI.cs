using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI characterNameTMP;
    public TextMeshProUGUI dialogueTMP;
    public CanvasGroup canvasGroup;
    public Image dialogueBoxBG;
    public Image nameTagBG;
    public TextMeshProUGUI nextIndicator;

    public float typingSpeed = 0.03f;

    private bool skipRequested;
    private bool isTyping;

    public static DialogueUI Create(Transform parent)
    {
        // ── Root Panel (전체 화면, 투명 — 클릭 수신용) ──
        var panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(parent, false);
        var panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var canvasGroup = panelObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        var ui = panelObj.AddComponent<DialogueUI>();
        ui.canvasGroup = canvasGroup;

        // ── 대화 상자 (하단 25%) ──
        var boxObj = new GameObject("DialogueBox");
        boxObj.transform.SetParent(panelObj.transform, false);
        var boxRect = boxObj.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0f, 0f);
        boxRect.anchorMax = new Vector2(1f, 0.28f);
        boxRect.offsetMin = new Vector2(40f, 20f);   // 좌우 40, 하단 20 여백
        boxRect.offsetMax = new Vector2(-40f, 0f);

        ui.dialogueBoxBG = boxObj.AddComponent<Image>();
        ui.dialogueBoxBG.color = new Color(0f, 0f, 0f, 0.75f);

        // 둥근 모서리 효과 (Sprite 없이 대체)
        var boxOutline = boxObj.AddComponent<Outline>();
        boxOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        boxOutline.effectDistance = new Vector2(2f, 2f);

        // ── 이름 태그 (대화 상자 위) ──
        var nameTagObj = new GameObject("NameTag");
        nameTagObj.transform.SetParent(panelObj.transform, false);
        var nameTagRect = nameTagObj.AddComponent<RectTransform>();
        // 대화 상자의 왼쪽 상단에 걸쳐 배치
        nameTagRect.anchorMin = new Vector2(0f, 0.28f);
        nameTagRect.anchorMax = new Vector2(0f, 0.28f);
        nameTagRect.pivot = new Vector2(0f, 0f);
        nameTagRect.anchoredPosition = new Vector2(60f, 4f);
        nameTagRect.sizeDelta = new Vector2(260f, 48f);

        ui.nameTagBG = nameTagObj.AddComponent<Image>();
        ui.nameTagBG.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);

        var nameTagOutline = nameTagObj.AddComponent<Outline>();
        nameTagOutline.effectColor = new Color(1f, 0.85f, 0.4f, 0.4f);
        nameTagOutline.effectDistance = new Vector2(1f, 1f);

        // 이름 텍스트
        var nameObj = new GameObject("CharacterName_TMP");
        nameObj.transform.SetParent(nameTagObj.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = Vector2.zero;
        nameRect.anchorMax = Vector2.one;
        nameRect.offsetMin = new Vector2(16f, 0f);
        nameRect.offsetMax = new Vector2(-16f, 0f);
        ui.characterNameTMP = nameObj.AddComponent<TextMeshProUGUI>();
        ui.characterNameTMP.fontSize = 28f;
        ui.characterNameTMP.fontStyle = FontStyles.Bold;
        ui.characterNameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        ui.characterNameTMP.color = new Color(1f, 0.9f, 0.5f);

        // ── 대사 텍스트 (대화 상자 안) ──
        var textObj = new GameObject("Dialogue_TMP");
        textObj.transform.SetParent(boxObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(30f, 20f);
        textRect.offsetMax = new Vector2(-30f, -12f);
        ui.dialogueTMP = textObj.AddComponent<TextMeshProUGUI>();
        ui.dialogueTMP.fontSize = 28f;
        ui.dialogueTMP.lineSpacing = 8f;
        ui.dialogueTMP.alignment = TextAlignmentOptions.TopLeft;
        ui.dialogueTMP.color = Color.white;
        ui.dialogueTMP.overflowMode = TextOverflowModes.Ellipsis;

        // ── 다음 표시 ▼ (대화 상자 우하단, 깜빡임) ──
        var indicatorObj = new GameObject("NextIndicator");
        indicatorObj.transform.SetParent(boxObj.transform, false);
        var indRect = indicatorObj.AddComponent<RectTransform>();
        indRect.anchorMin = new Vector2(1f, 0f);
        indRect.anchorMax = new Vector2(1f, 0f);
        indRect.pivot = new Vector2(1f, 0f);
        indRect.anchoredPosition = new Vector2(-20f, 12f);
        indRect.sizeDelta = new Vector2(40f, 30f);
        ui.nextIndicator = indicatorObj.AddComponent<TextMeshProUGUI>();
        ui.nextIndicator.text = "\u25bc"; // ▼
        ui.nextIndicator.fontSize = 22f;
        ui.nextIndicator.alignment = TextAlignmentOptions.BottomRight;
        ui.nextIndicator.color = new Color(1f, 1f, 1f, 0.6f);
        indicatorObj.SetActive(false);

        return ui;
    }

    public IEnumerator ShowDialogue(string character, string text)
    {
        canvasGroup.alpha = 1f;

        // 이름 태그
        bool hasName = !string.IsNullOrEmpty(character);
        nameTagBG.gameObject.SetActive(hasName);
        characterNameTMP.text = hasName ? character : "";

        // 다음 표시 숨김
        nextIndicator.gameObject.SetActive(false);

        // 타이핑 시작
        dialogueTMP.text = "";
        skipRequested = false;
        isTyping = true;

        for (int i = 0; i < text.Length; i++)
        {
            if (skipRequested)
            {
                dialogueTMP.text = text;
                break;
            }

            dialogueTMP.text += text[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        dialogueTMP.text = text;

        // 타이핑 완료 → ▼ 깜빡임 시작
        nextIndicator.gameObject.SetActive(true);
    }

    public IEnumerator WaitForInput()
    {
        // 한 프레임 대기 (이전 입력 무시)
        yield return null;

        float blinkTimer = 0f;
        while (true)
        {
            // ▼ 깜빡임
            blinkTimer += Time.deltaTime;
            if (nextIndicator.gameObject.activeSelf)
            {
                float a = Mathf.PingPong(blinkTimer * 2f, 1f) * 0.5f + 0.3f;
                nextIndicator.color = new Color(1f, 1f, 1f, a);
            }

            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        }

        nextIndicator.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isTyping) return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0))
        {
            skipRequested = true;
        }
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        isTyping = false;
        nextIndicator.gameObject.SetActive(false);
    }
}
