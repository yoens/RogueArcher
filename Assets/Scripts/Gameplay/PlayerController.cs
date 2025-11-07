using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    Camera _cam;
    Vector2 _moveInput;

    void Awake()
    {
        _cam = Camera.main;
    }

    // Input System에서 Move 액션 연결 예정
    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        // 1) 이동
        Vector3 dir = new Vector3(_moveInput.x, _moveInput.y, 0f);
        transform.position += dir * moveSpeed * Time.deltaTime;

        // 2) 마우스 바라보기
        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector3 lookDir = (mouseWorld - transform.position).normalized;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // 스프라이트 방향에 따라 -90은 조절
    }
}
