using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeUI : MonoBehaviour
{
    [SerializeField] public Image fadeImage;

    /// <summary>
    /// 페이드 아웃: 화면이 어두워짐 (alpha 0 → 1)
    /// </summary>
    public IEnumerator FadeOut(float duration, EaseType ease = EaseType.Linear)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        yield return Animate(0f, 1f, duration, ease);
    }

    /// <summary>
    /// 페이드 인: 화면이 밝아짐 (alpha 1 → 0)
    /// </summary>
    public IEnumerator FadeIn(float duration, EaseType ease = EaseType.Linear)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        yield return Animate(1f, 0f, duration, ease);
    }

    /// <summary>
    /// stateInt로 방향 결정: 1=FadeOut, 2=FadeIn
    /// </summary>
    public IEnumerator DoFade(int stateInt, float duration, EaseType ease = EaseType.Linear)
    {
        if (stateInt == 2)
            yield return FadeIn(duration, ease);
        else
            yield return FadeOut(duration, ease);
    }

    /// <summary>
    /// 즉시 검은 화면으로 세팅 (alpha=1)
    /// </summary>
    public void SetBlack()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        var c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
    }

    /// <summary>
    /// 즉시 투명으로 세팅 (alpha=0)
    /// </summary>
    public void SetClear()
    {
        var c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
    }

    private IEnumerator Animate(float from, float to, float duration, EaseType ease)
    {
        duration = Mathf.Max(duration, 0.01f);
        float elapsed = 0f;

        SetAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = Easing.Evaluate(ease, t);
            SetAlpha(Mathf.Lerp(from, to, easedT));
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        var c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
