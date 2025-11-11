using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyBomber : MonoBehaviour
{
    public float moveSpeed = 2f;

    [Header("폭탄 설치 조건")]
    public float dropDistance = 4f;      // 
    public float dropCooldown = 2f;      // 폭탄 설치 후 다시 설치까지 시간

    [Header("폭탄 프리팹")]
    public GameObject bombPrefab;        // 위에서 만든 Bomb 프리팹

    Transform _target;
    float _dropTimer = 0f;

    // SO에서 세팅 가능
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

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _target = player.transform;
    }

    void Update()
    {
        if (_target == null) return;

        _dropTimer -= Time.deltaTime;

        // 플레이어 쪽으로 이동은 계속
        Vector3 dir = (_target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // 거리 체크
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
        // 여기서 살짝 효과음/애니메이션 재생 가능
    }
}
