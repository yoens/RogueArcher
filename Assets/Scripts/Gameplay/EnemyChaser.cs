using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int contactDamage = 1;

    [Header("Pathfinding Lite")]
    public float avoidDistance = 1.5f;    // 레이 길이
    public float avoidStrength = 2f;      // 회피 힘
    public LayerMask obstacleMask;        // BlockTilemap 레이어 넣기
    public int rayCount = 8;              // 몇 방향 레이 쏠지

    Transform _target;
    Rigidbody2D _rb;

    public void Setup(EnemySO data)
    {
        if (data == null) return;

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
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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

        // 1) 기본 방향 = 플레이어 쪽
        Vector2 desiredDir = (_target.position - transform.position).normalized;

        // 2) PathfindingLite: 8-ray 기반 회피 방향 계산 (앞에서 만든 함수)
        Vector2 finalDir = GetSteeredDirection(desiredDir);

        // 3) 실제 이동은 velocity로 맡긴다
        _rb.velocity = finalDir * moveSpeed;
    }


    Vector2 GetSteeredDirection(Vector2 desiredDir)
    {
        // 장애물 마스크 없으면 그냥 직선 이동
        if (obstacleMask == 0)
            return desiredDir;

        // 기본 방향도 포함해서 주변 각도 체크 (예: -90 ~ +90 사이)
        float halfAngle = 90f;
        int rays = Mathf.Max(1, rayCount);
        float step = (halfAngle * 2f) / (rays - 1);

        Vector2 bestDir = desiredDir;
        float bestScore = -999f;

        for (int i = 0; i < rays; i++)
        {
            float angleOffset = -halfAngle + step * i;
            float angleRad = angleOffset * Mathf.Deg2Rad;

            // desiredDir 기준으로 회전된 방향
            Vector2 dir = new Vector2(
                desiredDir.x * Mathf.Cos(angleRad) - desiredDir.y * Mathf.Sin(angleRad),
                desiredDir.x * Mathf.Sin(angleRad) + desiredDir.y * Mathf.Cos(angleRad)
            ).normalized;

            // 레이 쏴보기
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, avoidDistance, obstacleMask);

            // hit가 없으면 0, 있으면 장애물까지의 거리로 점수 계산
            float score;
            if (hit.collider == null)
            {
                // 막힌 게 없으면 최고 점수 근처
                score = avoidDistance;
            }
            else
            {
                // 가까울수록 점수 낮게
                score = hit.distance;
            }

            // 조금이라도 플레이어 쪽과 비슷한 방향이면 가산점
            float align = Vector2.Dot(dir, desiredDir);  // -1 ~ 1
            score += align * avoidStrength;

            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        return bestDir.normalized;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Player")) return;

        if (col.collider.TryGetComponent<Health>(out var h))
        {
            h.Take(contactDamage);
        }
    }
}
