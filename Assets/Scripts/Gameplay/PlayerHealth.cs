using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    Health _health;
    public GameOverUI gameOverUI;
    public GameHUD gameHUD;

    void Awake()
    {
        _health = GetComponent<Health>();
        _health.destroyOnDie = false;
        _health.OnDie += OnPlayerDie;
        _health.OnDamaged += OnDamaged;

        if (gameHUD != null)
            gameHUD.SetHP(_health.currentHP, _health.maxHP);
    }
    void Start()
    {
        //  Start() 시점에는 Health.Awake()가 이미 실행 완료됨
        if (gameHUD != null)
            gameHUD.SetHP(_health.currentHP, _health.maxHP);
    }
    void OnDamaged()
    {
        if (gameHUD != null)
            gameHUD.SetHP(_health.currentHP, _health.maxHP);
    }

    //  MaxHP 강화 후에 외부에서 호출할 수 있는 함수
    public void RefreshHUD()
    {
        if (gameHUD != null)
            gameHUD.SetHP(_health.currentHP, _health.maxHP);
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
