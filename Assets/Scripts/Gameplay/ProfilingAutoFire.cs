using UnityEngine;

public class ProfilingAutoFire : MonoBehaviour
{
    public PlayerShooter shooter;
    public float interval = 0.001f;      // 발사 주기
    public int burstCount = 10000;         // 한 번에 10발 (사방으로)
    float _t;

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
            }
        }
    }
}