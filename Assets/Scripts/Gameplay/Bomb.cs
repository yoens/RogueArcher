using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float explodeDelay = 1.5f;
    public float radius = 2f;
    public int damage = 2;
    public LayerMask hitMask;

    public GameObject explosionEffectPrefab; 
    void Start()
    {
        Invoke(nameof(Explode), explodeDelay);
    }

    void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        var hits = Physics2D.OverlapCircleAll(transform.position, radius, hitMask);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Health>(out var h))
            {
                h.Take(damage);
            }
        }
        AudioManager.Instance?.PlaySFX("SFX_Explode");
       
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
