using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int _score = 0;
    public GameHUD hud;

    [Header("Score Upgrade")]
    public UpgradeUI upgradeUI;         // ������ ���� ��ȭ UI
    public PlayerStats playerStats;     // ������ ��ȭ ������ ���
    public UpgradeSO[] upgradePool;     //  ���� ���׷��̵�� Ǯ
    public int scorePerUpgrade = 50;    // 50������ ��ȭ
    int _nextUpgradeScore = 50;
    bool _upgradeOpen = false;


    

    SaveData _saveData;
    int _currentSlot = 0;
    public Difficulty difficulty = Difficulty.Normal;
    void Start()
    {
        
        AudioManager.Instance?.PlayBGM("BGM_Stage");
    }

    void Awake()
    {
        // ===== 측정용 (측정 끝나면 삭제) =====
        ObjectPool<Projectile>.BypassPool = false;   // 미적용 측정할 때
        // ObjectPool<Projectile>.BypassPool = false; // 적용 측정할 때 (기본값이라 생략 가능)
        // =====================================
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // ���θ޴����� ������ �� ��������
        _currentSlot = RunConfig.SaveSlotIndex;
        difficulty = RunConfig.Difficulty;

        // ���̺� �ε�
        _saveData = SaveSystem.Load(_currentSlot);
        Debug.Log($"[GameManager] Slot={_currentSlot}, BestScore={_saveData.bestScore}, TotalRuns={_saveData.totalRuns}, LastDiff={_saveData.lastDifficulty}");


    }

    public void AddScore(int amount)
    {
        float mul = GetScoreMul();
        int final = Mathf.RoundToInt(amount * mul);

        _score += final;

        if (hud != null)
            hud.SetScore(_score);

        CheckScoreUpgrade();
    }
    public void EndRun(bool isClear)
    {
        // �÷��� Ƚ�� ����
        _saveData.totalRuns++;

        _saveData.lastDifficulty = difficulty;
        // �ְ� ���� ����
        if (_score > _saveData.bestScore)
        {
            _saveData.bestScore = _score;
            _saveData.bestScoreDifficulty = difficulty;   
            Debug.Log($"[GameManager] New BestScore = {_saveData.bestScore} / {difficulty}");
        }


        SaveSystem.Save(_saveData, _currentSlot);
    }

    void CheckScoreUpgrade()
    {
        // ���� ���� ����
        if (_score < _nextUpgradeScore) return;

        // �̹� �ٸ� ��ȭâ ��������
        if (_upgradeOpen) return;

        // ���׷��̵� UI�� Ǯ�� ������ �׳� ���� ��ǥ�� �ø��� ��
        if (upgradeUI == null || upgradePool == null || upgradePool.Length == 0)
        {
            _nextUpgradeScore += scorePerUpgrade;
            return;
        }

        _upgradeOpen = true;

        // ���� 3�� �̱�
        var three = PickThree(upgradePool);

        // UI ����
        upgradeUI.Show(three, selected =>
        {
            ApplyUpgrade(selected);
            _upgradeOpen = false;
        });

        // ���� ��ǥ ����
        _nextUpgradeScore += scorePerUpgrade;
    }

    //  �̰� ���� SO ���
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

    // �״�� �ᵵ �Ǵ� ���� 3�� �̱�
    UpgradeSO[] PickThree(UpgradeSO[] pool)
    {
        if (pool == null || pool.Length == 0)
            return new UpgradeSO[0];

        // 1) Ǯ�� ����Ʈ�� ����
        List<UpgradeSO> list = new List<UpgradeSO>(pool);

        // 2) ���� ���� (Fisher-Yates ��Ÿ��)
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }

        // 3) �տ��� �ִ� 3�� �̱�
        int count = Mathf.Min(3, list.Count);
        UpgradeSO[] result = new UpgradeSO[count];
        for (int i = 0; i < count; i++)
            result[i] = list[i];

        return result;
    }
    public float GetEnemyHpMul()
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return 0.7f;  // �� ü�� 70%
            case Difficulty.Hard: return 1.4f;  // �� ü�� 140%
            default: return 1.0f;  // Normal
        }
    }

    public float GetEnemySpeedMul()
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return 0.9f;
            case Difficulty.Hard: return 1.2f;
            default: return 1.0f;
        }
    }

    public float GetEnemyDamageMul()
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return 0.7f;
            case Difficulty.Hard: return 1.3f;
            default: return 1.0f;
        }
    }

    public float GetScoreMul()
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return 0.8f;  // ������ ���� ���� �� ��
            case Difficulty.Hard: return 1.3f;  // ������� ���� �� ��
            default: return 1.0f;
        }
    }

    public int GetScore() => _score;
    public int GetBestScore() => _saveData != null ? _saveData.bestScore : 0;
    public int GetTotalRuns() => _saveData != null ? _saveData.totalRuns : 0;
}
