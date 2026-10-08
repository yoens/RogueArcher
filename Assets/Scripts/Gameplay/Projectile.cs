using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    public int damage = 1;

    float _timer;
    Vector3 _dir;
    ObjectPool<Projectile> _pool;

    int _pierceLeft;
    bool _inFlight;

    public void Init(ObjectPool<Projectile> pool)
    {
        _pool = pool;
    }

    public void Fire(
        Vector3 dir,
        int extraPierce,
        float finalSpeed,
        int finalDamage)
    {
        _dir = dir.normalized;
        _timer = 0f;
        _pierceLeft = 1 + extraPierce;

        speed = finalSpeed;
        damage = finalDamage;

        float angle =
            Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        _inFlight = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!_inFlight) return;

        transform.position +=
            _dir * speed * Time.deltaTime;

        _timer += Time.deltaTime;

        if (_timer >= lifeTime)
            Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_inFlight) return;

        if (other.CompareTag("Wall") ||
            other.CompareTag("Tile"))
        {
            Despawn();
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            AudioManager.Instance?.PlaySFX("SFX_HitEnemy");

            if (other.TryGetComponent<Health>(out var health))
                health.Take(damage);

            _pierceLeft--;

            if (_pierceLeft <= 0)
                Despawn();
        }
    }

    void Despawn()
    {
        if (!_inFlight) return;

        // 반환 또는 파괴 요청 전에 중복 처리를 차단
        _inFlight = false;

        if (_pool != null)
            _pool.Return(this);
        else
            gameObject.SetActive(false);
    }

    void OnDisable()
    {
        _inFlight = false;
    }
}