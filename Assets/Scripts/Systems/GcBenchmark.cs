using System;
using UnityEngine;
using Unity.Profiling;

// 발사 코드보다 먼저 이전 프레임의 결과를 집계
[DefaultExecutionOrder(-10000)]
public class GcBenchmark : MonoBehaviour
{
    public ProfilingAutoFire autoFire;

    public float warmup = 5f;
    public float duration = 60f;

    ProfilerRecorder _allocationRecorder;
    ObjectPool<Projectile> _projectilePool;
    int _startCreated;
    double _warmupStart;
    double _measurementStart;
    double _previousTime;

    bool _measuring;
    bool _done;

    int _startGc;
    int _startShots;
    int _frames;

    long _allocatedBytes;
    double _worstFrame;

    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        if (autoFire == null ||
            autoFire.shooter == null ||
            !autoFire.isActiveAndEnabled ||
            warmup < 0 || duration <= 0)
        {
            Debug.LogError(
                "Auto Fire와 Shooter 연결 및 측정 시간을 확인해 주세요.");
            enabled = false;
            return;
        }

        _allocationRecorder = ProfilerRecorder.StartNew(
            ProfilerCategory.Memory,
            "GC Allocated In Frame",
            1);

        if (!_allocationRecorder.Valid)
        {
            Debug.LogError(
                "할당량 카운터를 사용할 수 없습니다. " +
                "Editor 또는 Development Build에서 확인해 주세요.");
            enabled = false;
            return;
        }

        _warmupStart = Time.realtimeSinceStartupAsDouble;
    }

    void Update()
    {
        if (_done) return;

        if (autoFire.ScheduleOverloaded)
        {
            _done = true;
            _allocationRecorder.Stop();
            Debug.LogError("발사 일정 지연으로 이번 측정을 중단했습니다.");
            return;
        }

        double now = Time.realtimeSinceStartupAsDouble;

        if (!_measuring)
        {
            if (now - _warmupStart < warmup) return;
            if (_allocationRecorder.Count == 0) return;

            _measuring = true;
            _measurementStart = now;
            _previousTime = now;

            _startGc = GC.CollectionCount(0);
            _startShots = ProfilingAutoFire.TotalShots;
            _projectilePool = autoFire.shooter.GetPoolForTest();
            _startCreated = _projectilePool.CreatedCount;
            return;
        }

        // 이전 Update부터 현재 Update까지의 프레임 간격
        double frameSeconds = now - _previousTime;
        _previousTime = now;

        _frames++;
        _worstFrame = Math.Max(_worstFrame, frameSeconds);

        // 이전 프레임에서 발생한 관리 메모리 할당량
        _allocatedBytes += _allocationRecorder.LastValue;

        double elapsed = now - _measurementStart;
        if (elapsed < duration) return;

        _done = true;

        int gcCount = GC.CollectionCount(0) - _startGc;
        int shots = ProfilingAutoFire.TotalShots - _startShots;
        int createdDuringMeasurement =_projectilePool.CreatedCount - _startCreated;
        bool bypass = ObjectPool<Projectile>.BypassPool;

        // 종료 로그 자체의 할당은 측정에서 제외
        _allocationRecorder.Stop();
        autoFire.enabled = false;

        Debug.Log(
            $"[벤치마크] 실제 시간: {elapsed:F2}초 | " +
            $"GC.CollectionCount(0) 차이: {gcCount}회 | " +
            $"누적 관리 할당: {_allocatedBytes / 1048576.0:F3} MiB | " +
            $"발사: {shots}발 | " +
            $"실제 발사율: {shots / elapsed:F1}발/초 | " +
            $"측정 중 생성: {createdDuringMeasurement}개 | " +
            $"전체 생성: {_projectilePool.CreatedCount}개 | " +
            $"평균 FPS: {_frames / elapsed:F1} | " +
            $"최악 프레임 간격: {_worstFrame * 1000:F1}ms | " +
            $"Bypass: {bypass}");
    }

    void OnDestroy()
    {
        _allocationRecorder.Dispose();
    }
}