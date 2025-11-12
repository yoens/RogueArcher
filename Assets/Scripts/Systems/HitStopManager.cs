using UnityEngine;
using System.Collections;

public class HitStopManager : MonoBehaviour
{
    static HitStopManager _inst;
    static bool _busy = false;
    static float _lastTime = -999f;

    void Awake()
    {
        _inst = this;
    }

    /// <summary>
    /// 슬로모션형 히트스톱. 중복/연타 방지 포함.
    /// </summary>
    public static void TryDoHitStop(float duration = 0.06f, float slowMoScale = 0.15f, float cooldown = 0.25f)
    {
        if (_inst == null || _busy) return;
        if (Time.unscaledTime - _lastTime < cooldown) return;

        _inst.StartCoroutine(_inst.DoHitStop(duration, slowMoScale));
    }

    IEnumerator DoHitStop(float duration, float slowMoScale)
    {
        _busy = true;
        _lastTime = Time.unscaledTime;

        float oldScale = Time.timeScale;
        float oldFixed = Time.fixedDeltaTime;

        Time.timeScale = Mathf.Clamp(slowMoScale, 0.05f, 1f);
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // 물리 동기화

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = oldScale;
        Time.fixedDeltaTime = oldFixed;
        _busy = false;
    }
}