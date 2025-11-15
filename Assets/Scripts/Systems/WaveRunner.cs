using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveRunner : MonoBehaviour
{
    public WaveSO[] waves;
    public UpgradeSO[] upgradePool;
    public VictoryUI victoryUI;

    public float waveInterval = 3f;
    public float spawnRadius = 5f;
    public float offscreenMargin = 2f;

    public GameHUD gameHUD;
    public UpgradeUI upgradeUI;
    public PlayerStats playerStats;

    public EnemySO bossData;
    public float bossSpawnRadius = 30f;

    [Header("Spawn Safety")]
    public LayerMask obstacleMask;       // BlockTilemap 레이어
    public float spawnCheckRadius = 0.6f;
    public int spawnMaxTry = 10;

    [Header("Boss Arena")]
    public GameObject stageEnv;          // 일반 웨이브 맵 루트
    public GameObject bossEnv;           // 보스 아레나 맵 루트
    public Transform bossPlayerSpawn;    // 보스 페이즈 시작 시 플레이어 위치
    public Transform bossSpawnPoint;     // 보스 스폰 위치

    List<GameObject> _alive = new();
    int _current = 0;
    Transform _player;

    // ========== Upgrade 랜덤 3개 뽑기 ==========
    UpgradeSO[] PickThree(UpgradeSO[] pool)
    {
        if (pool == null || pool.Length == 0)
            return new UpgradeSO[0];

        List<UpgradeSO> list = new List<UpgradeSO>(pool);
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }

        int count = Mathf.Min(3, list.Count);
        UpgradeSO[] result = new UpgradeSO[count];
        for (int i = 0; i < count; i++)
            result[i] = list[i];

        return result;
    }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        //  시작할 때는 Stage만 켜두고 BossEnv는 끔
        if (stageEnv != null) stageEnv.SetActive(true);
        if (bossEnv != null) bossEnv.SetActive(false);

        StartCoroutine(Run());
    }

    // ========== 메인 루프 ==========
    IEnumerator Run()
    {
        while (_current < waves.Length)
        {
            var wave = waves[_current];

            if (gameHUD != null)
                gameHUD.SetWave(_current + 1, waves.Length);

            if (wave.startDelay > 0)
                yield return new WaitForSeconds(wave.startDelay);

            yield return StartCoroutine(SpawnWave(wave));
            yield return StartCoroutine(WaitAllDead());

            // 웨이브 끝 → 강화 선택
            if (upgradeUI != null)
            {
                bool done = false;
                var three = PickThree(upgradePool);

                upgradeUI.Show(three, selectedUpgrade =>
                {
                    ApplyUpgrade(selectedUpgrade);
                    done = true;
                });

                yield return new WaitUntil(() => done);
            }

            // 다음 웨이브까지 대기 + 타이머 표시
            float t = waveInterval;
            while (t > 0)
            {
                if (gameHUD != null)
                    gameHUD.SetTimer(t);
                t -= Time.deltaTime;
                yield return null;
            }
            if (gameHUD != null)
                gameHUD.SetTimer(0);

            _current++;
        }

        //모든 Wave 끝 → 보스 페이즈 시작
        if (bossData != null && bossData.prefab != null)
        {
            StartBossPhase();
        }

        Debug.Log("모든 WaveSO + Boss 소진!");
    }

    // ========== 보스 페이즈 시작 ==========
    void StartBossPhase()
    {
        AudioManager.Instance?.PlayBGM("BGM_Boss");
        // 1) 환경 교체
        if (stageEnv != null) stageEnv.SetActive(false);
        if (bossEnv != null) bossEnv.SetActive(true);

        // 2) 플레이어 위치 이동
        if (_player != null && bossPlayerSpawn != null)
            _player.position = bossPlayerSpawn.position;

        // 3) 보스 스폰 위치 결정
        Vector2 spawnPos;
        if (bossSpawnPoint != null)
            spawnPos = bossSpawnPoint.position;
        else if (_player != null)
            spawnPos = (Vector2)_player.position + Vector2.up * 3f;
        else
            spawnPos = Vector2.zero;

        // 4) 보스 생성 + 세팅
        var boss = Instantiate(bossData.prefab, spawnPos, Quaternion.identity);

        var bossCtrl = boss.GetComponent<BossEnemy>();
        if (bossCtrl != null)
            bossCtrl.Setup(bossData);

        // 5) 보스 HP바 / 클리어 처리
        Health h = boss.GetComponent<Health>();
        if (gameHUD != null && h != null)
        {
            gameHUD.ShowBossHP(h);
            h.OnDie += () =>
            {
                gameHUD.HideBossHP();
                if (victoryUI != null) victoryUI.Show();
            };
        }

        if (gameHUD != null)
            gameHUD.ShowBossAlert("BOSS!!");
    }

    // ========== 웨이브 스폰 ==========
    IEnumerator SpawnWave(WaveSO wave)
    {
        foreach (var info in wave.enemies)
        {
            yield return StartCoroutine(SpawnEnemyGroup(info));
        }
    }

    IEnumerator SpawnEnemyGroup(WaveEnemyInfo info)
    {
        if (info.enemy == null || info.enemy.prefab == null)
            yield break;

        for (int i = 0; i < info.count; i++)
        {
            Vector2 pos = FindSafeSpawnPosition();

            var enemy = Instantiate(info.enemy.prefab, pos, Quaternion.identity);

            var chaser = enemy.GetComponent<EnemyChaser>();
            if (chaser != null)
                chaser.Setup(info.enemy);

            var shooter = enemy.GetComponent<EnemyShooter>();
            if (shooter != null)
                shooter.Setup(info.enemy);

            var bomber = enemy.GetComponent<EnemyBomber>();
            if (bomber != null)
                bomber.Setup(info.enemy);

            var h = enemy.GetComponent<Health>();
            if (h != null)
            {
                h.OnDie += () =>
                {
                    _alive.Remove(enemy);
                    if (GameManager.Instance != null)
                        GameManager.Instance.AddScore(10);
                };
            }

            _alive.Add(enemy);

            yield return new WaitForSeconds(info.spawnInterval);
        }
    }

    IEnumerator WaitAllDead()
    {
        while (true)
        {
            _alive.RemoveAll(e => e == null);
            if (_alive.Count == 0)
                break;
            yield return null;
        }
    }

    // ========== 강화 적용 ==========
    void ApplyUpgrade(UpgradeSO upgrade)
    {
        if (playerStats == null || upgrade == null) return;

        switch (upgrade.type)
        {
            case UpgradeType.MoveSpeed:
                playerStats.AddMoveSpeed(upgrade.floatValue);
                break;
            case UpgradeType.FireRate:
                playerStats.AddFireRate(upgrade.floatValue);
                break;
            case UpgradeType.Pierce:
                playerStats.AddPierce(upgrade.intValue);
                break;
            case UpgradeType.Damage:
                playerStats.AddDamage(upgrade.intValue);
                break;
            case UpgradeType.MaxHP:
                playerStats.AddMaxHP(upgrade.intValue);
                break;
            case UpgradeType.ProjectileSpeed:
                playerStats.AddProjectileSpeed(upgrade.floatValue);
                break;
        }

        Debug.Log($"Upgrade picked: {upgrade.displayName}");
    }

    // ========== 스폰 위치 계산 ==========
    Vector2 FindSafeSpawnPosition()
    {
        for (int i = 0; i < spawnMaxTry; i++)
        {
            Vector2 candidate = GetOffscreenPosition();

            bool blocked = Physics2D.OverlapCircle(candidate, spawnCheckRadius, obstacleMask);
            if (!blocked)
                return candidate;
        }

        return GetOffscreenPosition();
    }

    Vector2 GetOffscreenPosition()
    {
        Camera cam = Camera.main;
        Vector2 center = _player != null ? (Vector2)_player.position : Vector2.zero;

        if (cam == null)
            return center + Random.insideUnitCircle.normalized * spawnRadius;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float margin = offscreenMargin;

        int side = Random.Range(0, 4);
        Vector2 pos = center;

        switch (side)
        {
            case 0: // 위
                pos = center + new Vector2(Random.Range(-halfW, halfW), halfH + margin);
                break;
            case 1: // 아래
                pos = center + new Vector2(Random.Range(-halfW, halfW), -halfH - margin);
                break;
            case 2: // 오른쪽
                pos = center + new Vector2(halfW + margin, Random.Range(-halfH, halfH));
                break;
            case 3: // 왼쪽
                pos = center + new Vector2(-halfW - margin, Random.Range(-halfH, halfH));
                break;
        }

        return pos;
    }
}
