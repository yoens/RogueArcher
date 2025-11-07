using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 0.2f;
    public int prewarm = 20;

    Camera _cam;
    float _cd;
    ObjectPool<Projectile> _pool;
    PlayerStats _stats;  // ★ 추가

    void Awake()
    {
        _cam = Camera.main;
        _pool = new ObjectPool<Projectile>(projectilePrefab, prewarm);
        _stats = GetComponent<PlayerStats>();   // Player에 붙어있는 거 가져오기
    }

    void Update()
    {
        _cd -= Time.deltaTime;
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // ★ 최종 쿨다운 계산
        float finalCooldown = fireCooldown;
        if (_stats != null)
        {
            // fireRateBonus가 0.2면 0.2초 빨라진다 이런 느낌
            finalCooldown = Mathf.Max(0.05f, fireCooldown - _stats.fireRateBonus);
        }

        if (_cd > 0f) return;
        _cd = finalCooldown;

        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;
        Vector3 dir = (mouseWorld - transform.position).normalized;

        var proj = _pool.Get(firePoint.position, Quaternion.identity);
        proj.Init(_pool);

        // ★ 관통 개수도 여기서 넘겨줄 수 있음 (Projectile에 세팅해주기)
        int pierce = _stats != null ? _stats.pierceBonus : 0;
        proj.Fire(dir, pierce);
    }
}
