using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI characterNameTMP;
    [SerializeField] public TextMeshProUGUI dialogueTMP;
    [SerializeField] public CanvasGroup canvasGroup;
    [SerializeField] public Image dialogueBoxBG;
    [SerializeField] public Image nameTagBG;
    [SerializeField] public TextMeshProUGUI nextIndicator;

    public float typingSpeed = 0.03f;

    private bool skipRequested;
    private bool isTyping;

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
