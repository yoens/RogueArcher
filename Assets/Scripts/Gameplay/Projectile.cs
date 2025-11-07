using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifeTime = 2f;
    public int damage = 1;

    float _timer;
    Vector3 _dir;

    public void Fire(Vector3 dir)
    {
        _dir = dir.normalized;
        _timer = 0f;
    }

    void Update()
    {
        transform.position += _dir * speed * Time.deltaTime;
        _timer += Time.deltaTime;
        if (_timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: Enemy에 Damage 주기
        // if (other.TryGetComponent<EnemyHealth>(out var hp)) hp.Take(damage);
        gameObject.SetActive(false);
    }
}
