using UnityEngine;

/// <summary>
/// DOTween 스타일 이징 타입. 대표적 6가지.
/// </summary>
public enum EaseType
{
    Linear,      // 선형 — 균일한 속도
    InSine,      // 사인 가속 — 느리게 시작
    OutSine,     // 사인 감속 — 느리게 끝
    InOutSine,   // 사인 S커브 — 느리게 시작+끝
    InCubic,     // 큐빅 가속 — 강하게 가속
    OutCubic,    // 큐빅 감속 — 강하게 감속
}

/// <summary>
/// 이징 함수 유틸리티. 입력 t(0~1) → 출력 (0~1).
/// </summary>
public static class Easing
{
    private const float PI = Mathf.PI;
    private const float HALF_PI = Mathf.PI * 0.5f;

    /// <summary>
    /// EaseType에 따른 이징 값 반환.
    /// </summary>
    public static float Evaluate(EaseType type, float t)
    {
        t = Mathf.Clamp01(t);

        switch (type)
        {
            case EaseType.Linear:
                return t;

            case EaseType.InSine:
                return 1f - Mathf.Cos(t * HALF_PI);

            case EaseType.OutSine:
                return Mathf.Sin(t * HALF_PI);

            case EaseType.InOutSine:
                return -(Mathf.Cos(PI * t) - 1f) * 0.5f;

            case EaseType.InCubic:
                return t * t * t;

            case EaseType.OutCubic:
                float f = 1f - t;
                return 1f - f * f * f;

            default:
                return t;
        }
    }
}
