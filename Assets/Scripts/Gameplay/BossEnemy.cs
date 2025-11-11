using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossEnemy : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float keepDistance = 4f;
    public GameObject projectilePrefab;
    public float fireInterval = 1.2f;
    public int projectileBurst = 6;   // 한 번에 여러 방향으로 쏘기

    Transform _target;
    float _timer;

    public void Setup(EnemySO data)
    {
        if (data == null) return;

        moveSpeed = data.moveSpeed;
        if (TryGetComponent<Health>(out var h))
        {
            h.maxHP = data.maxHP;
            h.currentHP = data.maxHP;
            h.destroyOnDie = false; // 보스는 죽을 때 우리가 처리
            h.OnDie += OnBossDie;
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

        // 플레이어와 일정거리 유지하면서
        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist > keepDistance)
        {
            Vector3 dir = (_target.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }

        // 주기적으로 탄 발사
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            FireBurst();
            _timer = fireInterval;
        }
    }

    void FireBurst()
    {
        if (projectilePrefab == null) return;

        // 원형으로 뿌리기
        float angleStep = 360f / projectileBurst;
        for (int i = 0; i < projectileBurst; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            if (proj.TryGetComponent<Rigidbody2D>(out var rb))
                rb.velocity = dir * 4f; // 보스 탄속
        }
    }

    void OnBossDie()
    {
        Debug.Log("Boss Dead");
        // 여기서 GameManager에 클리어 알리기
        if (GameManager.Instance != null)
        {
            // 점수 보상 같은 거
            GameManager.Instance.AddScore(100);
        }
        Destroy(gameObject);
    }
}
