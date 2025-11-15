using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    public int damage = 1;

    float _timer;
    Vector3 _dir;
    ObjectPool<Projectile> _pool;

    int _pierceLeft = 0;

    public void Init(ObjectPool<Projectile> pool)
    {
        _pool = pool;
    }

    // dir: 방향
    // extraPierce: 플레이어 스탯에서 온 관통 보너스
    // finalSpeed: 스탯 적용된 탄속
    // finalDamage: 스탯 적용된 데미지
    public void Fire(Vector3 dir, int extraPierce, float finalSpeed, int finalDamage)
    {
        _dir = dir.normalized;
        _timer = 0f;

        // 기본 1타 + 보너스 관통
        _pierceLeft = 1 + extraPierce;

        // 여기서 실제 사용할 값 세팅
        speed = finalSpeed;
        damage = finalDamage;

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
        if (other.CompareTag("Wall") || other.CompareTag("Tile"))
        {
            Despawn();
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<Health>(out var h))
                h.Take(damage);

            _pierceLeft--;
            if (_pierceLeft <= 0)
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
