using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyShooter : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float keepDistance = 4f;
    public float fireInterval = 1.5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public int contactDamage = 1;

    Transform _target;
    Rigidbody2D _rb;
    float _timer;

    [Header("Avoidance")]
    public float avoidDistance = 1.2f;
    public float avoidStrength = 2f;
    public LayerMask obstacleMask;

    public void Setup(EnemySO data)
    {
        moveSpeed = data.moveSpeed;
        contactDamage = data.contactDamage;

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
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _target = p.transform;
    }

    void FixedUpdate()
    {
        if (_target == null) return;

        float dist = Vector2.Distance(transform.position, _target.position);

        // 1) 기본 방향 (플레이어 쪽)
        Vector2 desiredDir = (_target.position - transform.position).normalized;
        Vector2 moveDir = Vector2.zero;

        // 2) 일정 거리보다 멀면 접근
        if (dist > keepDistance)
        {
            moveDir = desiredDir;

            // 장애물 감지
            RaycastHit2D hit = Physics2D.Raycast(transform.position, desiredDir, avoidDistance, obstacleMask);
            if (hit.collider != null)
            {
                Vector2 avoidDir = Vector2.Perpendicular(desiredDir);
                if (Vector2.Dot(avoidDir, hit.normal) < 0)
                    avoidDir = -avoidDir;

                moveDir += avoidDir * avoidStrength;
                moveDir.Normalize();
            }
        }

        // 3) 이동: 가까우면 멈추고, 멀면 이동
        _rb.velocity = moveDir * moveSpeed;

        // 4) 바라보기
        Vector2 look = (_target.position - transform.position).normalized;
        float angle = Mathf.Atan2(look.y, look.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle); 

        // 5) 공격
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Fire();
            _timer = fireInterval;
        }
    }

    void Fire()
    {
        if (projectilePrefab == null || _target == null) return;

        var obj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 dir = (_target.position - transform.position).normalized;

        if (obj.TryGetComponent<EnemyProjectile>(out var ep))
            ep.Fire(dir);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {
            if (col.collider.TryGetComponent<Health>(out var h))
                h.Take(contactDamage);
        }
    }
}
