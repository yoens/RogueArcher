using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float presentSpeed;

    Camera _cam;
    Vector2 _moveInput;
    Rigidbody2D _rb;
    PlayerStats _stats;

    [Header("Body Sprite")]
    public SpriteRenderer spriteRenderer;
    public Sprite rightSprite;
    public Sprite leftSprite;

    [Header("Bow / Arm")]
    public Transform bowPivot;            // 활 회전 피벗(자식 오브젝트)
    public SpriteRenderer bowRenderer;    // 활/팔 SpriteRenderer
    public Sprite bowRightSprite;
    public Sprite bowLeftSprite;

    [Header("Bow Offsets")]
    public Vector2 bowOffsetRight = new Vector2(0.25f, 0.0f);
    public Vector2 bowOffsetLeft = new Vector2(-0.25f, 0.0f);

    void Awake()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
    }

    // 이동 입력
    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        if (Mouse.current == null || _cam == null) return;

        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        // 1) 몸 기준으로 마우스가 왼쪽/오른쪽인지
        float dx = mouseWorld.x - transform.position.x;
        bool isRight = dx >= 0f;

        // 2) 몸 스프라이트 교체
        if (spriteRenderer != null)
        {
            if (isRight && rightSprite != null)
                spriteRenderer.sprite = rightSprite;
            else if (!isRight && leftSprite != null)
                spriteRenderer.sprite = leftSprite;
        }

        // 3) 활 스프라이트/위치 설정
        if (bowRenderer != null)
        {
            if (isRight && bowRightSprite != null)
                bowRenderer.sprite = bowRightSprite;
            else if (!isRight && bowLeftSprite != null)
                bowRenderer.sprite = bowLeftSprite;
        }

        if (bowPivot != null)
        {
            // 오른쪽/왼쪽에 따라 피벗 위치도 바꿔주기
            bowPivot.localPosition = isRight ? (Vector3)bowOffsetRight
                                             : (Vector3)bowOffsetLeft;

            // 4) 활 회전 (피벗 기준)
            Vector3 origin = bowPivot.position;
            Vector2 dir = (mouseWorld - origin);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 왼쪽 스프라이트는 기본이 -X 방향이라 180도 보정
            if (!isRight)
                angle += 180f;

            bowPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void FixedUpdate()
    {
        Vector2 dir = _moveInput.normalized;

        float finalSpeed = moveSpeed;
        if (_stats != null)
            finalSpeed += _stats.moveSpeedBonus;

        presentSpeed = finalSpeed;
        _rb.MovePosition(_rb.position + dir * finalSpeed * Time.fixedDeltaTime);
    }
}
