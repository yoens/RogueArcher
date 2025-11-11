using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyShooter : MonoBehaviour
{
    public float moveSpeed = 1.5f;     // 너무 안 다가오면 심심하니까 약간은 움직이게
    public float keepDistance = 4f;    // 이 거리까지만 접근
    public float fireInterval = 1.5f;  // 몇 초마다 쏠지
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public int contactDamage = 1;      // 너무 붙었을 때 플레이어 데미지

    Transform _target;
    float _timer;

    // SO에서 부를 초기화 함수
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

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _target = player.transform;
    }

    void Update()
    {
        if (_target == null) return;

        // 1) 일정 거리까지만 다가오기
        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist > keepDistance)
        {
            Vector3 dir = (_target.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }

        // 2) 플레이어 바라보기 (선택)
        Vector3 lookDir = (_target.position - transform.position).normalized;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // 3) 쿨타임 돌리고
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

        var projObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var dir = (_target.position - transform.position).normalized;

        if (projObj.TryGetComponent<EnemyProjectile>(out var ep))
        {
            ep.Fire(dir);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.TryGetComponent<Health>(out var h))
                h.Take(contactDamage);
        }
    }
}
