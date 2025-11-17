using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int _score = 0;
    public GameHUD hud;

    [Header("Score Upgrade")]
    public UpgradeUI upgradeUI;         // 점수로 열릴 강화 UI
    public PlayerStats playerStats;     // 실제로 강화 적용할 대상
    public UpgradeSO[] upgradePool;     //  점수 업그레이드용 풀
    public int scorePerUpgrade = 50;    // 50점마다 강화
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
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // 메인메뉴에서 선택한 값 가져오기
        _currentSlot = RunConfig.SaveSlotIndex;
        difficulty = RunConfig.Difficulty;

        // 세이브 로드
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
        // 플레이 횟수 증가
        _saveData.totalRuns++;

        _saveData.lastDifficulty = difficulty;
        // 최고 점수 갱신
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
        // 점수 아직 부족
        if (_score < _nextUpgradeScore) return;

        // 이미 다른 강화창 열려있음
        if (_upgradeOpen) return;

        // 업그레이드 UI나 풀이 없으면 그냥 다음 목표만 올리고 끝
        if (upgradeUI == null || upgradePool == null || upgradePool.Length == 0)
        {
            _nextUpgradeScore += scorePerUpgrade;
            return;
        }

        _upgradeOpen = true;

        // 랜덤 3개 뽑기
        var three = PickThree(upgradePool);

        // UI 열기
        upgradeUI.Show(three, selected =>
        {
            ApplyUpgrade(selected);
            _upgradeOpen = false;
        });

        // 다음 목표 점수
        _nextUpgradeScore += scorePerUpgrade;
    }

    //  이건 이제 SO 기반
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

    // 그대로 써도 되는 랜덤 3개 뽑기
    UpgradeSO[] PickThree(UpgradeSO[] pool)
    {
        if (pool == null || pool.Length == 0)
            return new UpgradeSO[0];

        // 1) 풀을 리스트로 복사
        List<UpgradeSO> list = new List<UpgradeSO>(pool);

        // 2) 랜덤 셔플 (Fisher-Yates 스타일)
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }

        // 3) 앞에서 최대 3개 뽑기
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
            case Difficulty.Easy: return 0.7f;  // 적 체력 70%
            case Difficulty.Hard: return 1.4f;  // 적 체력 140%
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
            case Difficulty.Easy: return 0.8f;  // 쉬움은 점수 조금 덜 줌
            case Difficulty.Hard: return 1.3f;  // 어려움은 점수 더 줌
            default: return 1.0f;
        }
    }

    public int GetScore() => _score;
    public int GetBestScore() => _saveData != null ? _saveData.bestScore : 0;
    public int GetTotalRuns() => _saveData != null ? _saveData.totalRuns : 0;
}
