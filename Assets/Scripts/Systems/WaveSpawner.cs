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
    public float waveInterval = 3f;
    public float radius = 5f;

    public UpgradeUI upgradeUI;
    public PlayerStats playerStats;

    List<GameObject> _alive = new();
    int _currentWave = 0;

    void Start()
    {
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        while (_currentWave < waves.Length)
        {
            var wave = waves[_currentWave];

            yield return StartCoroutine(SpawnWave(wave));
            yield return StartCoroutine(WaitAllDead());

            Debug.Log($" Wave {_currentWave + 1} cleared, showing upgrade panel...");

            bool selected = false;
            if (upgradeUI != null)
            {
                Debug.Log(" upgradeUI.Show() called");
                upgradeUI.Show(index =>
                {
                    ApplyUpgrade(index);
                    selected = true;
                    Debug.Log(" Upgrade selected: {index}");
                });

                yield return new WaitUntil(() => selected);
            }
            else
            {
                Debug.LogWarning(" upgradeUI is NULL — panel not assigned!");
            }

            yield return new WaitForSeconds(waveInterval);

            _currentWave++;
        }

        Debug.Log("모든 웨이브 클리어!");
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
            // 여기 추가!
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
