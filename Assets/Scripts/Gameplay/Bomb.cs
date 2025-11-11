using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float explodeDelay = 1.5f;
    public float radius = 2f;
    public int damage = 2;
    public LayerMask hitMask;

    public GameObject explosionEffectPrefab;  // ★ 추가

    void Start()
    {
        Invoke(nameof(Explode), explodeDelay);
    }

    void Explode()
    {
        // 1) 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2) 데미지 처리
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Health>(out var h))
            {
                h.Take(damage);
            }
        }

        // 3) 폭탄 본체 제거
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
