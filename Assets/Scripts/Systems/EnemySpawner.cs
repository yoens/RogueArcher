using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float interval = 3f;
    public float radius = 5f;

    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= interval)
        {
            _timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
  
        Vector2 pos = Random.insideUnitCircle.normalized * radius;
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }
}
