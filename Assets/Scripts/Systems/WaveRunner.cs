using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveRunner : MonoBehaviour
{
    public WaveSO[] waves;        // 인스펙터에서 WaveSO 3개 넣어두면 끝
    public float waveInterval = 3f;
    public float spawnRadius = 5f;

    public GameHUD gameHUD;
    public UpgradeUI upgradeUI;
    public PlayerStats playerStats;

    List<GameObject> _alive = new();
    int _current = 0;

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (_current < waves.Length)
        {
            var wave = waves[_current];

            // HUD 웨이브 표시
            if (gameHUD != null)
                gameHUD.SetWave(_current + 1, waves.Length);

            // 시작 전 대기
            if (wave.startDelay > 0)
                yield return new WaitForSeconds(wave.startDelay);

            // 이 웨이브 소환
            yield return StartCoroutine(SpawnWave(wave));

            // 다 죽을 때까지
            yield return StartCoroutine(WaitAllDead());

            // 강화 UI
            if (upgradeUI != null)
            {
                bool done = false;
                upgradeUI.Show(index =>
                {
                    ApplyUpgrade(index);
                    done = true;
                });
                yield return new WaitUntil(() => done);
            }

            // 웨이브 간 대기 + HUD 타이머
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

        Debug.Log("모든 WaveSO 소진!");
    }

    IEnumerator SpawnWave(WaveSO wave)
    {
        // WaveSO 안에 여러 종류가 들어있을 수 있음
        foreach (var info in wave.enemies)
        {
            // 적 종류마다 따로 스폰 코루틴
            yield return StartCoroutine(SpawnEnemyGroup(info));
        }
    }

    IEnumerator SpawnEnemyGroup(WaveEnemyInfo info)
    {
        for (int i = 0; i < info.count; i++)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            Vector2 center = player != null ? (Vector2)player.transform.position : Vector2.zero;
            Vector2 pos = center + Random.insideUnitCircle.normalized * spawnRadius;

            // EnemySO → prefab 꺼내서 생성
            var enemy = Instantiate(info.enemy.prefab, pos, Quaternion.identity);

            var h = enemy.GetComponent<Health>();
            if (h != null)
            {
                h.OnDie += () => { _alive.Remove(enemy); };
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

    void ApplyUpgrade(int index)
    {
        if (playerStats == null) return;

        switch (index)
        {
            case 0: playerStats.AddMoveSpeed(1f); break;
            case 1: playerStats.AddFireRate(0.2f); break;
            case 2: playerStats.AddPierce(1); break;
        }
    }
}
