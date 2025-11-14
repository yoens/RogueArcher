using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;   // 기본 속도
    public float presentSpeed;

    Camera _cam;
    Vector2 _moveInput;
    Rigidbody2D _rb;
    PlayerStats _stats;            // 추가

    void Awake()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();   // 있으면 가져옴
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        // 마우스 바라보기
        if (Mouse.current != null)
        {
            Vector3 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0f;

            Vector3 lookDir = (mouseWorld - transform.position).normalized;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle ); // 스프라이트 방향 따라 조절
        }
    }

    void FixedUpdate()
    {
        Vector2 dir = _moveInput.normalized;

        // 스탯 있으면 보너스 더해주기
        float finalSpeed = moveSpeed;
        if (_stats != null)
            finalSpeed += _stats.moveSpeedBonus;

        presentSpeed = finalSpeed;
        _rb.MovePosition(_rb.position + dir * finalSpeed * Time.fixedDeltaTime);
    }
}
