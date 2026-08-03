using UnityEngine;
using System;

// 측정용 벤치마크 (측정 끝나면 삭제)
public class GcBenchmark : MonoBehaviour
{
    public float warmup = 5f;        // 처음 5초 버림 (로딩/풀 확장)
    public float duration = 60f;

    float _t;
    bool _measuring, _done;
    int _startGc, _frames;
    int _startShots;
    float _worstFrame;
    long _lastMem, _allocated;
    void Awake()
    {
        Application.targetFrameRate = 60;    // 베이스라인 프레임 수 통일
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        if (_done) return;
        _t += Time.unscaledDeltaTime;

        if (!_measuring)
        {
            if (_t >= warmup)
            {
                _measuring = true;
                _t = 0f;
                _startGc = GC.CollectionCount(0);
                _lastMem = GC.GetTotalMemory(false);
                _startShots = ProfilingAutoFire.TotalShots;
            }
            return;
        }

        _frames++;
        _worstFrame = Mathf.Max(_worstFrame, Time.unscaledDeltaTime);

        long mem = GC.GetTotalMemory(false);
        if (mem > _lastMem) _allocated += mem - _lastMem;  // 증가분 누적 = 총 할당량
        _lastMem = mem;

        if (_t >= duration)
        {
            _done = true;
            int gcCount = GC.CollectionCount(0) - _startGc;
            int shots = ProfilingAutoFire.TotalShots - _startShots;
            Debug.Log($"[벤치마크] {duration}초 | GC: {gcCount}회 | " +
                      $"총 할당: {_allocated / 1024f:F0} KB | " +
                      $"발사: {shots}발 | " +
                      $"평균 FPS: {_frames / _t:F1} | 최악 프레임: {_worstFrame * 1000:F1}ms | " +
                      $"Bypass: {ObjectPool<Projectile>.BypassPool}");
        }
    }
}