using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform firePoint;      // 총알이 나갈 위치
    public float fireCooldown = 0.2f;

    Camera _cam;
    float _cd;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        _cd -= Time.deltaTime;
    }

    // Input System에서 Fire 액션이 들어오면 이 함수 호출
    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (_cd > 0f) return;

        _cd = fireCooldown;

        // 마우스 방향 계산
        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;
        Vector3 dir = (mouseWorld - transform.position).normalized;

        // 투사체 생성
        var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.Fire(dir);
    }
}
