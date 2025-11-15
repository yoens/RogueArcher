using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBomber : MonoBehaviour
{
    public float moveSpeed = 2f;

    [Header("Bomb Drop")]
    public float dropDistance = 4f;
    public float dropCooldown = 2f;
    public GameObject bombPrefab;

    float _dropTimer;
    Transform _target;
    Rigidbody2D _rb;

    [Header("Avoidance")]
    public float avoidDistance = 1.2f;
    public float avoidStrength = 2f;
    public LayerMask obstacleMask;

    public void Setup(EnemySO data)
    {
        if (data == null) return;

        moveSpeed = data.moveSpeed;

        if (TryGetComponent<Health>(out var h))
        {
            h.maxHP = data.maxHP;
            h.currentHP = data.maxHP;
        }
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        var col = GetComponent<Collider2D>();
        col.isTrigger = false;
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _target = player.transform;
    }

    void FixedUpdate()
    {
        if (_target == null) return;

        _dropTimer -= Time.deltaTime;

        // 1) 기본 추적 방향
        Vector2 desiredDir = (_target.position - transform.position).normalized;

        // 2) 장애물 감지 + 회피
        Vector2 dir = desiredDir;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDir, avoidDistance, obstacleMask);
        if (hit.collider != null)
        {
            Vector2 avoid = Vector2.Perpendicular(desiredDir);
            if (Vector2.Dot(avoid, hit.normal) < 0)
                avoid = -avoid;

            dir += avoid * avoidStrength;
            dir.Normalize();
        }

        // 3) 이동은 velocity로
        _rb.velocity = dir * moveSpeed;

        // 4) 폭탄 설치
        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist <= dropDistance && _dropTimer <= 0f)
        {
            DropBomb();
            _dropTimer = dropCooldown;
        }
    }

    void DropBomb()
    {
        if (bombPrefab == null) return;
        Instantiate(bombPrefab, transform.position, Quaternion.identity);
    }
}
