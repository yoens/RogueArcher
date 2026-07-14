using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int contactDamage = 1;

    [Header("Pathfinding Lite")]
    public float avoidDistance = 1.5f;   
    public float avoidStrength = 2f;    
    public LayerMask obstacleMask;        
    public int rayCount = 8;            

    Transform _target;
    Rigidbody2D _rb;

    public void Setup(EnemySO data)
    {
        if (data == null) return;

        float hpMul = GameManager.Instance != null ? GameManager.Instance.GetEnemyHpMul() : 1f;
        float spdMul = GameManager.Instance != null ? GameManager.Instance.GetEnemySpeedMul() : 1f;
        float dmgMul = GameManager.Instance != null ? GameManager.Instance.GetEnemyDamageMul() : 1f;

        moveSpeed = data.moveSpeed * spdMul;
        contactDamage = Mathf.RoundToInt(data.contactDamage * dmgMul);

        if (TryGetComponent<Health>(out var h))
        {
            h.maxHP = Mathf.RoundToInt(data.maxHP * hpMul);
            h.currentHP = h.maxHP;
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

        Vector2 desiredDir = (_target.position - transform.position).normalized;

        Vector2 finalDir = GetSteeredDirection(desiredDir);

        _rb.velocity = finalDir * moveSpeed;
    }


    Vector2 GetSteeredDirection(Vector2 desiredDir)
    {
        if (obstacleMask == 0)
            return desiredDir;

        float halfAngle = 90f;
        int rays = Mathf.Max(1, rayCount);
        float step = (halfAngle * 2f) / (rays - 1);

        Vector2 bestDir = desiredDir;
        float bestScore = -999f;

        for (int i = 0; i < rays; i++)
        {
            float angleOffset = -halfAngle + step * i;
            float angleRad = angleOffset * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                desiredDir.x * Mathf.Cos(angleRad) - desiredDir.y * Mathf.Sin(angleRad),
                desiredDir.x * Mathf.Sin(angleRad) + desiredDir.y * Mathf.Cos(angleRad)
            ).normalized;

           
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, avoidDistance, obstacleMask);

            float score;
            if (hit.collider == null)
            {
                score = avoidDistance;
            }
            else
            {
                score = hit.distance;
            }

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
