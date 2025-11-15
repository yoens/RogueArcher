using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 0.2f;
    public int prewarm = 20;
    public float baseProjectileSpeed = 12f;  // 기본 탄속 분리
    public int baseDamage = 1;               // 기본 데미지 분리

    Camera _cam;
    float _cd;
    ObjectPool<Projectile> _pool;
    PlayerStats _stats;  // Player에 붙어있는 거 가져오기

    void Awake()
    {
        _cam = Camera.main;
        _pool = new ObjectPool<Projectile>(projectilePrefab, prewarm);
        _stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        _cd -= Time.deltaTime;
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // 1) 최종 쿨타임 계산
        float finalCooldown = fireCooldown;
        if (_stats != null)
            finalCooldown = Mathf.Max(0.05f, fireCooldown - _stats.fireRateBonus);

        if (_cd > 0f) return;
        _cd = finalCooldown;

        // 2) 마우스 방향 계산
        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;
        Vector3 dir = (mouseWorld - transform.position).normalized;

        // 3) 풀에서 탄 꺼내기
        var proj = _pool.Get(firePoint.position, Quaternion.identity);
        proj.Init(_pool);

        // 4) 스탯 적용
        int bonusPierce = _stats != null ? _stats.pierceBonus : 0;
        float finalSpeed = baseProjectileSpeed;
        int finalDamage = baseDamage;

        if (_stats != null)
        {
            finalSpeed += _stats.projectileSpeedBonus;
            finalDamage += _stats.damageBonus;
        }

        // 5) 발사
        proj.Fire(dir, bonusPierce, finalSpeed, finalDamage);
        AudioManager.Instance?.PlaySFX("SFX_Shoot");
    }
}
