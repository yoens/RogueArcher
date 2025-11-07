using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    public int damage = 1;

    float _timer;
    Vector3 _dir;
    ObjectPool<Projectile> _pool;

    int _pierceLeft = 0;   // 남은 관통 수

    public void Init(ObjectPool<Projectile> pool)
    {
        _pool = pool;
    }

    // ★ 관통 수를 함께 받아오기
    public void Fire(Vector3 dir, int extraPierce = 0)
    {
        _dir = dir.normalized;
        _timer = 0f;
        // 기본으로 1번은 맞게 하고, 거기에 보너스만큼 추가
        _pierceLeft = 1 + extraPierce;
        gameObject.SetActive(true);
    }

    void Update()
    {
        transform.position += _dir * speed * Time.deltaTime;
        _timer += Time.deltaTime;
        if (_timer >= lifeTime)
        {
            Despawn();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어는 무시
        if (!other.CompareTag("Enemy"))
            return;

        if (other.TryGetComponent<Health>(out var h))
        {
            h.Take(damage);
        }

        _pierceLeft--;
        if (_pierceLeft <= 0)
        {
            Despawn();
        }
    }

    void Despawn()
    {
        if (_pool != null)
            _pool.Return(this);
        else
            gameObject.SetActive(false);
    }
}
