using UnityEngine;

[RequireComponent(typeof(Health), typeof(SpriteRenderer))]
public class BossEnemy : MonoBehaviour
{
    public enum BossState { Phase1, Phase2, Dead }

    [Header("Sprites")]
    public Sprite phase1Sprite;
    public Sprite phase2Sprite;

    [Header("Common")]
    public float keepDistance = 4f;
    public GameObject projectilePrefab;

    [Header("Phase Threshold")]
    [Range(0.1f, 0.9f)] public float phase2Threshold = 0.5f; // HP 50% 미만이면 전환

    [Header("Phase1")]
    public float moveSpeedP1 = 1.5f;
    public float fireIntervalP1 = 1.2f;
    public int burstCountP1 = 6;
    public float projectileSpeedP1 = 4f;

    [Header("Phase2")]
    public float moveSpeedP2 = 2.0f;
    public float fireIntervalP2 = 0.8f;
    public int burstCountP2 = 10;
    public float projectileSpeedP2 = 5f;
    public float spiralDeltaDeg = 12f;  // 스파이럴 회전 각도

    Transform _target;
    SpriteRenderer _sr;
    Health _health;
    BossState _state = BossState.Phase1;

    float _fireTimer;
    float _spiralOffsetDeg; // Phase2에서 회전 각도 누적

    // WaveRunner에서 SO로 주입
    public void Setup(EnemySO data)
    {
        if (data == null) return;

        if (TryGetComponent(out _health))
        {
            _health.maxHP = data.maxHP;
            _health.currentHP = data.maxHP;
            _health.destroyOnDie = false;
            _health.OnDie += OnBossDie;
            _health.OnHPChanged += OnBossHPChanged;
        }

        moveSpeedP1 = data.moveSpeed;
    }

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _target = player.transform;
        EnterPhase(BossState.Phase1);
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDie -= OnBossDie;
            _health.OnHPChanged -= OnBossHPChanged;
        }
    }

    void Update()
    {
        if (_state == BossState.Dead || _target == null) return;

        MoveTick();
        FireTick();
    }

    // ========= State Machine =========
    void EnterPhase(BossState next)
    {
        _state = next;
        _fireTimer = 0f;

        if (_sr != null)
        {
            if (_state == BossState.Phase1 && phase1Sprite != null)
                _sr.sprite = phase1Sprite;
            else if (_state == BossState.Phase2 && phase2Sprite != null)
                _sr.sprite = phase2Sprite;
        }

        var hud = FindObjectOfType<GameHUD>();
        if (hud != null && _state == BossState.Phase2)
            hud.ShowBossAlert("PHASE 2!");

        if (_state == BossState.Phase2)
        {
            CameraShake.Instance?.Shake(3.5f, 0.25f);
            HitStopManager.TryDoHitStop(0.05f, slowMoScale: 0.15f, cooldown: 0.5f);
        }
    }

    void OnBossHPChanged(int cur, int max)
    {
        if (_state == BossState.Dead) return;

        if (_state != BossState.Phase2)
        {
            float ratio = (max > 0) ? (float)cur / max : 0f;
            if (ratio <= phase2Threshold)
            {
                EnterPhase(BossState.Phase2);
            }
        }
    }

    void OnBossDie()
    {
        _state = BossState.Dead;

        Debug.Log("Boss Dead");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(100);
            GameManager.Instance.EndRun(true);
        }

        Destroy(gameObject);
    }

    // ========= Movement / Fire =========
    void MoveTick()
    {
        float moveSpeed = (_state == BossState.Phase2) ? moveSpeedP2 : moveSpeedP1;

        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist > keepDistance)
        {
            Vector3 dir = (_target.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
    }

    void FireTick()
    {
        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0f) return;

        if (_state == BossState.Phase1)
        {
            FireBurstCircle(burstCountP1, projectileSpeedP1);
            _fireTimer = fireIntervalP1;
        }
        else if (_state == BossState.Phase2)
        {
            FireSpiral(burstCountP2, projectileSpeedP2, _spiralOffsetDeg);
            _spiralOffsetDeg += spiralDeltaDeg;
            _fireTimer = fireIntervalP2;
        }
    }

    void FireBurstCircle(int count, float speed)
    {
        if (projectilePrefab == null) return;

        float step = 360f / Mathf.Max(1, count);
        for (int i = 0; i < count; i++)
        {
            float angleRad = (step * i) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            if (proj.TryGetComponent<Rigidbody2D>(out var rb))
                rb.velocity = dir * speed;
        }
    }

    void FireSpiral(int count, float speed, float offsetDeg)
    {
        if (projectilePrefab == null) return;

        float step = 360f / Mathf.Max(1, count);
        for (int i = 0; i < count; i++)
        {
            float angleDeg = offsetDeg + step * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            if (proj.TryGetComponent<Rigidbody2D>(out var rb))
                rb.velocity = dir * speed;
        }
    }
}
