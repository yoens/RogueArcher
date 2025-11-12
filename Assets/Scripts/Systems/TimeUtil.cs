using System.Collections;
using UnityEngine;

public static class TimeUtil
{
    public static IEnumerator HitStop(float duration = 0.1f)
    {
        float originalTime = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalTime;
    }
}
