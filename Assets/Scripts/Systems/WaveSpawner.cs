using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveEnemy
{
    public GameObject enemyPrefab;
    public int count = 5;
    public float spawnInterval = 0.5f;
}

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public WaveEnemy[] waves;
    public float waveInterval = 3f;   // 웨이브 끝나고 다음 웨이브까지 쉬는 시간
    public float radius = 5f;         // 플레이어 주변 몇 m 떨어져서 소환할지

    public UpgradeUI upgradeUI;      
    public PlayerStats playerStats;
    public GameHUD gameHUD;          // ★ HUD 연결

    List<GameObject> _alive = new();
    int _currentWave = 0;

    void Start()
    {
        // 게임 시작할 때 현재 웨이브 표시
        if (gameHUD != null)
            gameHUD.SetWave(1, waves.Length);

        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        while (_currentWave < waves.Length)
        {
            var wave = waves[_currentWave];

            // 1) 이 웨이브 소환
            yield return StartCoroutine(SpawnWave(wave));
            // 2) 다 죽을 때까지 기다림
            yield return StartCoroutine(WaitAllDead());

            Debug.Log($"✅ Wave {_currentWave + 1} cleared, showing upgrade panel...");

            // 3) 웨이브 끝났으니까 강화 선택
            bool selected = false;
            if (upgradeUI != null)
            {
                upgradeUI.Show(index =>
                {
                    ApplyUpgrade(index);
                    selected = true;
                });

                // 선택할 때까지 기다린다
                yield return new WaitUntil(() => selected);
            }

            // 4) 휴식 타이머 표시 (다음 웨이브까지 남은 시간)
            if (gameHUD != null)
            {
                // 쉬는 동안 카운트다운
                float t = waveInterval;
                while (t > 0f)
                {
                    gameHUD.SetTimer(t);
                    t -= Time.deltaTime;
                    yield return null;
                }
                // 쉬기 끝났으면 0:00으로
                gameHUD.SetTimer(0);
            }
            else
            {
                // HUD 없으면 그냥 기다리기
                yield return new WaitForSeconds(waveInterval);
            }

            _currentWave++;

            // 다음 웨이브 번호 HUD에 표시
            if (_currentWave < waves.Length && gameHUD != null)
            {
                gameHUD.SetWave(_currentWave + 1, waves.Length);
            }
        }

        Debug.Log("모든 웨이브 클리어!");

        // 다 끝났을 때 타이머 0으로
        if (gameHUD != null)
        {
            gameHUD.SetTimer(0);
        }
    }

    IEnumerator SpawnWave(WaveEnemy wave)
    {
        for (int i = 0; i < wave.count; i++)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector2 center = player != null ? (Vector2)player.transform.position : Vector2.zero;
            Vector2 pos = center + Random.insideUnitCircle.normalized * radius;

            var enemy = Instantiate(wave.enemyPrefab, pos, Quaternion.identity);

            var h = enemy.GetComponent<Health>();
            if (h != null)
            {
                h.OnDie += () => { _alive.Remove(enemy); };
            }

            _alive.Add(enemy);

            yield return new WaitForSeconds(wave.spawnInterval);
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

    void ApplyUpgrade(int index)
    {
        if (playerStats == null) return;

        switch (index)
        {
            case 0:
                playerStats.AddMoveSpeed(1f);
                break;
            case 1:
                playerStats.AddFireRate(0.2f);
                break;
            case 2:
                playerStats.AddPierce(1);
                break;
        }

        Debug.Log($"Upgrade picked: {index}");
    }
}
