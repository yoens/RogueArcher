using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int contactDamage = 1;
    Transform _target;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _target = player.transform;
    }

    void Update()
    {
        if (_target == null) return;

        Vector3 dir = (_target.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    // ★ 트리거 충돌로 바꾼다
    void OnTriggerEnter2D(Collider2D other)
    {
        // Player 아니면 무시
        if (!other.CompareTag("Player"))
            return;

        if (other.TryGetComponent<Health>(out var h))
        {
            h.Take(contactDamage);
        }
    }
}
