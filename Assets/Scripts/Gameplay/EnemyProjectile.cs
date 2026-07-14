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
        if (other.CompareTag("Wall") || other.CompareTag("Tile"))
        {
            Despawn();
            return;
        }
       
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Health>(out var h))
            {
                h.Take(damage);
            }
            Despawn();
        }
       
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
