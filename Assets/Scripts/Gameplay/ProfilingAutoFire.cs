using UnityEngine;

public class ProfilingAutoFire : MonoBehaviour
{
    public static int TotalShots = 0;   // ← [추가 ①] 발사 카운터 (static)

    public PlayerShooter shooter;
    public bool bypassPool = false;
    public float interval = 0.02f;
    public int burstCount = 20;
    float _t;

    void Awake()
    {
        ObjectPool<Projectile>.BypassPool = bypassPool;
        TotalShots = 0;                 // ← [추가 ②] 시작 시 초기화
    }

    void Update()
    {
        if (shooter == null) return;
        _t += Time.deltaTime;
        if (_t >= interval)
        {
            _t = 0f;
            var pool = shooter.GetPoolForTest();
            for (int i = 0; i < burstCount; i++)
            {
                var proj = pool.Get(shooter.firePoint.position, Quaternion.identity);
                proj.Init(pool);
                proj.Fire(Random.insideUnitCircle.normalized, 0, 12f, 1);
                TotalShots++;           // ← [추가 ③] 발사할 때마다 카운트
            }
        }
    }
}