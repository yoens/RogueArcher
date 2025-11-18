using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveRunner : MonoBehaviour
{
    [Header("Wave Sets")]
    public WaveSetSO easyWaveSet;
    public WaveSetSO normalWaveSet;
    public WaveSetSO hardWaveSet;

    WaveSO[] _waves;
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

    [Header("Play Area")]
    public BoxCollider2D playAreaCollider;

    [Header("Fallback Spawn Points")]
    public Transform[] fallbackSpawnPoints;

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

        Difficulty diff = GameManager.Instance != null ? GameManager.Instance.difficulty : Difficulty.Normal;

        switch (diff)
        {
            case Difficulty.Easy:
                _waves = (easyWaveSet != null) ? easyWaveSet.waves : null;
                break;
            case Difficulty.Hard:
                _waves = (hardWaveSet != null) ? hardWaveSet.waves : null;
                break;
            default:
                _waves = (normalWaveSet != null) ? normalWaveSet.waves : null;
                break;
        }

        if (_waves == null || _waves.Length == 0)
        {
            Debug.LogWarning("[WaveRunner] 선택된 난이도에 WaveSet이 비어 있습니다. 기본 Normal WaveSO 배열을 사용합니다.");
            // 혹시 대비용: normalWaveSet이 있다면 그걸 쓰고, 그것마저 없으면 그냥 아무 것도 안 함
            if (normalWaveSet != null && normalWaveSet.waves != null && normalWaveSet.waves.Length > 0)
                _waves = normalWaveSet.waves;
        }

        StartCoroutine(Run());
    }

    // ========== 메인 루프 ==========
    IEnumerator Run()
    {
        // 혹시 _waves가 비어 있으면 그냥 종료
        if (_waves == null || _waves.Length == 0)
        {
            Debug.LogWarning("[WaveRunner] _waves가 비어 있음. Run() 종료.");
            yield break;
        }

        while (_current < _waves.Length)
        {
            var wave = _waves[_current];

            if (gameHUD != null)
                gameHUD.SetWave(_current + 1, _waves.Length);

            if (wave.startDelay > 0)
                yield return new WaitForSeconds(wave.startDelay);

            yield return StartCoroutine(SpawnWave(wave));
            yield return StartCoroutine(WaitAllDead());

            // 강화 UI
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

            // 웨이브 간 대기
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

        // 모든 웨이브 후 보스
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

                    UpdateEnemyCountUI();
                };
            }

            _alive.Add(enemy);
            UpdateEnemyCountUI();
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
        UpdateEnemyCountUI();
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

    Vector2 FindSafeSpawnPosition()
    {
        for (int i = 0; i < spawnMaxTry; i++)
        {
            // 0) 예전처럼 화면 밖 기준으로 스폰 위치 뽑기
            Vector2 candidate = GetOffscreenPosition();

            // 1) 플레이 영역 안쪽으로 클램프
            candidate = ClampInsidePlayArea(candidate);

            // 2) 장애물 체크
            bool blockedCircle = Physics2D.OverlapCircle(
                candidate,
                spawnCheckRadius,
                obstacleMask
            );

            bool blockedBox = Physics2D.BoxCast(
                candidate,
                Vector2.one * spawnCheckRadius * 1.2f,
                0f,
                Vector2.zero,
                0f,
                obstacleMask
            );

            bool blockedRay = Physics2D.Raycast(
                candidate,
                Vector2.down,
                0.2f,
                obstacleMask
            );

            if (!blockedCircle && !blockedBox && !blockedRay)
            {
                return candidate;
            }
        }

        // 여기부터 "정말 다 막혔을 때" 처리
        // 1순위: 직접 지정한 fallback 스폰 포인트들
        if (fallbackSpawnPoints != null && fallbackSpawnPoints.Length > 0)
        {
            Transform p = fallbackSpawnPoints[Random.Range(0, fallbackSpawnPoints.Length)];
            if (p != null)
                return p.position;
        }

        // 2순위: 그래도 없으면 그냥 안쪽으로 클램프된 위치
        Vector2 fallback = ClampInsidePlayArea(GetOffscreenPosition());
        return fallback;
    }
    Vector2 ClampInsidePlayArea(Vector2 pos)
    {
        if (playAreaCollider == null)
            return pos;

        Bounds b = playAreaCollider.bounds;

        // 벽에서 살짝 안쪽으로 들어오게 margin
        float margin = 0.5f;

        float x = Mathf.Clamp(pos.x, b.min.x + margin, b.max.x - margin);
        float y = Mathf.Clamp(pos.y, b.min.y + margin, b.max.y - margin);

        return new Vector2(x, y);
    }

    Vector2 GetOffscreenPosition()
    {
        Camera cam = Camera.main;
        Vector2 center = _player != null ? (Vector2)_player.position : Vector2.zero;

        // 화면 밖으로 내보낼 거리 (인스펙터에서 1.5 ~ 3 정도 추천)
        float sideMargin = offscreenMargin;

        // 맵 벽에서 살짝 안쪽으로 스폰시키기 위한 여유
        float wallMargin = 0.8f;

        // 1) 카메라 기준으로 "화면 밖" 좌표 먼저 계산
        if (cam != null)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            int side = Random.Range(0, 4);
            Vector2 pos = center;

            switch (side)
            {
                case 0: // 위
                    pos = center + new Vector2(
                        Random.Range(-halfW, halfW),
                        halfH + sideMargin
                    );
                    break;
                case 1: // 아래
                    pos = center + new Vector2(
                        Random.Range(-halfW, halfW),
                        -halfH - sideMargin
                    );
                    break;
                case 2: // 오른쪽
                    pos = center + new Vector2(
                        halfW + sideMargin,
                        Random.Range(-halfH, halfH)
                    );
                    break;
                case 3: // 왼쪽
                    pos = center + new Vector2(
                        -halfW - sideMargin,
                        Random.Range(-halfH, halfH)
                    );
                    break;
            }

            // 2) 플레이 영역(BoxCollider2D) 안으로 강제 클램프
            if (playAreaCollider != null)
            {
                Bounds b = playAreaCollider.bounds;

                pos.x = Mathf.Clamp(pos.x, b.min.x + wallMargin, b.max.x - wallMargin);
                pos.y = Mathf.Clamp(pos.y, b.min.y + wallMargin, b.max.y - wallMargin);
            }

            return pos;
        }

        // 3) 카메라가 없으면, 플레이 영역 안쪽에서만 랜덤
        if (playAreaCollider != null)
        {
            Bounds b = playAreaCollider.bounds;
            wallMargin = 0.8f;

            float minX = b.min.x + wallMargin;
            float maxX = b.max.x - wallMargin;
            float minY = b.min.y + wallMargin;
            float maxY = b.max.y - wallMargin;

            return new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );
        }

        // 4) 최악의 경우(플레이어/카메라/콜라이더 다 없을 때) 예전 방식
        return center + Random.insideUnitCircle.normalized * spawnRadius;
    }
    void UpdateEnemyCountUI()
    {
        if (gameHUD != null)
            gameHUD.SetEnemyCount(_alive.Count);
    }
}
