using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveRunner : MonoBehaviour
{
    public WaveSO[] waves;        // 인스펙터에서 WaveSO들 넣기
    public UpgradeSO[] upgradePool;
    public VictoryUI victoryUI;

    public float waveInterval = 3f;
    public float spawnRadius = 5f;       // ← 예비용(카메라 못 찾을 때)
    public float offscreenMargin = 2f;   // 화면 밖으로 얼마나 내보낼지

    public GameHUD gameHUD;
    public UpgradeUI upgradeUI;
    public PlayerStats playerStats;

    public EnemySO bossData;        // ← 인스펙터에서 Boss SO 넣기
    public float bossSpawnRadius = 30f;

    List<GameObject> _alive = new();
    int _current = 0;

    Transform _player; // 플레이어 캐싱

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

        StartCoroutine(Run());
    }

    void SpawnBoss()
    {
        Vector2 center = _player != null ? (Vector2)_player.position : Vector2.zero;
        Vector2 pos = center + Random.insideUnitCircle.normalized * bossSpawnRadius;

        var boss = Instantiate(bossData.prefab, pos, Quaternion.identity);

        var bossCtrl = boss.GetComponent<BossEnemy>();
        if (bossCtrl != null)
            bossCtrl.Setup(bossData);

        Health h = boss.GetComponent<Health>();
        if (gameHUD != null && h != null)
        {
            gameHUD.ShowBossHP(h);   // ← 여기에서 슬라이더 min/max/현재값 세팅 + 구독
            h.OnDie += () =>
            {
                gameHUD.HideBossHP();
                if (victoryUI != null) victoryUI.Show();
            };
        }


        if (gameHUD != null)
            gameHUD.ShowBossAlert("BOSS!!");
    }

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

        if (bossData != null && bossData.prefab != null)
        {
            SpawnBoss();
        }

        Debug.Log("모든 WaveSO + Boss 소진!");
    }

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
            // ★ 여기서 화면 밖 위치 계산해서 쓴다
            Vector2 pos = GetOffscreenPosition();

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

    //  화면 밖 위치 계산하는 헬퍼
    Vector2 GetOffscreenPosition()
    {
        Camera cam = Camera.main;
        Vector2 center = _player != null ? (Vector2)_player.position : Vector2.zero;

        // 카메라 못 찾으면 예전 방식으로
        if (cam == null)
            return center + Random.insideUnitCircle.normalized * spawnRadius;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // 화면 밖 띄울 거리
        float margin = offscreenMargin;

        // 네 면 중 하나 선택
        int side = Random.Range(0, 4);
        Vector2 pos = center;

        switch (side)
        {
            // 위
            case 0:
                pos = center + new Vector2(
                    Random.Range(-halfW, halfW),
                    halfH + margin
                );
                break;
            // 아래
            case 1:
                pos = center + new Vector2(
                    Random.Range(-halfW, halfW),
                    -halfH - margin
                );
                break;
            // 오른쪽
            case 2:
                pos = center + new Vector2(
                    halfW + margin,
                    Random.Range(-halfH, halfH)
                );
                break;
            // 왼쪽
            case 3:
                pos = center + new Vector2(
                    -halfW - margin,
                    Random.Range(-halfH, halfH)
                );
                break;
        }

        return pos;
    }
}
