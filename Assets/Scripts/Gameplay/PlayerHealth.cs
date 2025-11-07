using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    Health _health;
    public GameOverUI gameOverUI;

    void Awake()
    {
        _health = GetComponent<Health>();
        _health.destroyOnDie = false;   // ★ 플레이어는 파괴하지 않기
        _health.OnDie += OnPlayerDie;
    }

    void OnPlayerDie()
    {
        Debug.Log("Player Dead");
        if (gameOverUI != null)
            gameOverUI.Show();
        else
            Time.timeScale = 0f;
    }
}
