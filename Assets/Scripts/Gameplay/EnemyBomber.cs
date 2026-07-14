using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBomber : MonoBehaviour
{
    public float moveSpeed = 2f;

    [Header("Bomb Throw")]
    public float dropDistance = 4f;    
    public float dropCooldown = 2f;
    public GameObject bombPrefab;

    float _dropTimer;
    Transform _target;
    Rigidbody2D _rb;

    [Header("������ ����")]
    public float throwForce = 6f;      

    [Header("Avoidance")]
    public float avoidDistance = 1.2f;
    public float avoidStrength = 2f;
    public LayerMask obstacleMask;

    public void Setup(EnemySO data)
    {
        if (data == null) return;

        float hpMul = GameManager.Instance != null ? GameManager.Instance.GetEnemyHpMul() : 1f;
        float spdMul = GameManager.Instance != null ? GameManager.Instance.GetEnemySpeedMul() : 1f;
        float dmgMul = GameManager.Instance != null ? GameManager.Instance.GetEnemyDamageMul() : 1f;

        moveSpeed = data.moveSpeed * spdMul;

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

        
        Vector2 desiredDir = (_target.position - transform.position).normalized;

        
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

       
        _rb.velocity = dir * moveSpeed;

       
        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist <= dropDistance && _dropTimer <= 0f)
        {
            ThrowBomb();
            _dropTimer = dropCooldown;
        }
    }

   
    void ThrowBomb()
    {
        if (bombPrefab == null || _target == null) return;

       
        Vector2 dir = (_target.position - transform.position).normalized;

        var bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

        if (bomb.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = dir * throwForce;
        }

        
    }
}
