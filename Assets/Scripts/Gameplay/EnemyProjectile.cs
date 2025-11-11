using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 2f;
    public int damage = 1;

    Vector3 _dir;

    public void Fire(Vector3 dir)
    {
        _dir = dir.normalized;
        Invoke(nameof(Despawn), lifeTime);
    }

    void Update()
    {
        transform.position += _dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ★ 플레이어만 맞춤
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Health>(out var h))
            {
                h.Take(damage);
            }
            Despawn();
        }
        // Enemy나 다른 건 무시
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
