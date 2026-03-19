using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceUI : MonoBehaviour
{
    [SerializeField] public CanvasGroup canvasGroup;
    [SerializeField] public Button[] buttons;
    [SerializeField] public TextMeshProUGUI[] buttonTexts;

    public int SelectedIndex { get; private set; } = -1;
    private bool chosen;

    public IEnumerator ShowChoices(List<string> options)
    {
        chosen = false;
        SelectedIndex = -1;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < options.Count)
            {
                buttons[i].gameObject.SetActive(true);
                buttonTexts[i].text = options[i];
                int idx = i;
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() => OnSelect(idx));
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }

        while (!chosen)
            yield return null;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnSelect(int idx)
    {
        SelectedIndex = idx;
        chosen = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        foreach (var btn in buttons)
            btn.gameObject.SetActive(false);
    }
}
