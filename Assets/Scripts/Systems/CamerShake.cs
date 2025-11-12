using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    CinemachineVirtualCamera _vcam;
    CinemachineBasicMultiChannelPerlin _perlin;
    Coroutine _running;

    void Awake()
    {
        Instance = this;
        _vcam = GetComponent<CinemachineVirtualCamera>();
        _perlin = _vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        HardReset();
    }

    void OnEnable() => HardReset();
    void OnDisable() => HardReset();

    public void Shake(float amplitude = 3f, float duration = 0.25f)
    {
        if (_perlin == null) return;
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(CoShake(amplitude, duration));
    }

    IEnumerator CoShake(float amp, float dur)
    {
        _perlin.m_AmplitudeGain = amp;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / dur); // 감쇠
            _perlin.m_AmplitudeGain = amp * k;
            yield return null;
        }
        HardReset();
        _running = null;
    }

    public void HardReset()
    {
        if (_perlin != null)
        {
            _perlin.m_AmplitudeGain = 0f;
            // 원한다면 Frequency도 초기값으로
            // _perlin.m_FrequencyGain = 1f;
        }
        // VCam은 항상 0도, Z만 -10 같은 고정값 유지
        transform.localRotation = Quaternion.identity;
    }
}
