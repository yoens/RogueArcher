using UnityEngine;

[DefaultExecutionOrder(-9000)]
public class ProfilingAutoFire : MonoBehaviour
{
    public static int TotalShots = 0;

    public PlayerShooter shooter;
    public bool bypassPool = false;

    [Min(0.001f)]
    public float interval = 0.02f;

    [Min(1)]
    public int burstCount = 2;

    public bool ScheduleOverloaded { get; private set; }

    double _timer;
    double _previousTime;
    bool _clockStarted;

    const int MaxBatchesPerFrame = 100;

    void Awake()
    {
        ObjectPool<Projectile>.BypassPool = bypassPool;

        TotalShots = 0;
        ScheduleOverloaded = false;

        _timer = 0;
        _previousTime = 0;
        _clockStarted = false;
    }

    void Update()
    {
        if (shooter == null) return;

        if (interval <= 0 || burstCount <= 0)
        {
            Debug.LogError(
                "Interval과 Burst Count는 0보다 커야 합니다.");

            enabled = false;
            return;
        }

        double now = Time.realtimeSinceStartupAsDouble;

        // 시작 전 로딩 시간을 발사 일정에서 제외
        if (!_clockStarted)
        {
            _clockStarted = true;
            _previousTime = now;
            return;
        }

        double elapsed = now - _previousTime;
        _previousTime = now;
        _timer += elapsed;

        if (_timer < interval) return;

        int batches = 0;
        var pool = shooter.GetPoolForTest();

        // 남은 시간을 유지하며 밀린 발사 횟수를 보충
        while (_timer >= interval)
        {
            // 과도한 보충 발사로 실행이 멈추는 것을 방지
            if (batches >= MaxBatchesPerFrame)
            {
                ScheduleOverloaded = true;
                enabled = false;

                Debug.LogError(
                    $"발사 일정 지연으로 중단: " +
                    $"이번 프레임 간격={elapsed:F3}초, " +
                    $"남은 누적 시간={_timer:F3}초, " +
                    $"Interval={interval:F3}, " +
                    $"BurstCount={burstCount}");

                return;
            }

            _timer -= interval;
            batches++;

            for (int i = 0; i < burstCount; i++)
            {
                var proj = pool.Get(
                    shooter.firePoint.position,
                    Quaternion.identity);

                proj.Init(pool);

                proj.Fire(
                    Random.insideUnitCircle.normalized,
                    0,
                    12f,
                    1);

                TotalShots++;
            }
        }
    }
}