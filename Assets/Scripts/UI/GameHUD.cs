using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameHUD : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject bossAlertRoot;
    public TextMeshProUGUI bossAlertText;

    public GameObject bossHPRoot;
    public Slider bossHPSlider;
    public TextMeshProUGUI bossHPText;

    Health _boundBossHealth; // 현재 바인딩된 보스 HP
    void Awake()
    {

        if (bossAlertRoot) bossAlertRoot.SetActive(false);
        if (bossHPRoot) bossHPRoot.SetActive(false);
    }

    public void SetHP(int current, int max)
    {
        if (hpText != null)
            hpText.text = $"HP: {current} / {max}";
    }

    public void SetWave(int current, int total)
    {
        if (waveText != null)
            waveText.text = $"Wave {current} / {total}";
    }

    public void SetTimer(float seconds)
    {
        if (timerText == null) return;

        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void SetBoss(string name = "BOSS")
    {
        if (waveText != null)
            waveText.text = name;
    }

    public void ShowBossAlert(string msg = "BOSS!!!")
    {
        if (bossAlertRoot == null) return;

        StopAllCoroutines();

        bossAlertRoot.SetActive(true);
        if (bossAlertText != null)
            bossAlertText.text = msg;

        StartCoroutine(HideBossAlertAfterDelay(2f));
    }
    public void ShowBossHP(Health bossHealth)
    {
        if (bossHPRoot == null || bossHPSlider == null || bossHealth == null) return;

        if (_boundBossHealth != null)
            _boundBossHealth.OnHPChanged -= OnBossHPChanged; // 중복구독 방지

        _boundBossHealth = bossHealth;

        bossHPRoot.SetActive(true);
        bossHPSlider.minValue = 0;
        bossHPSlider.maxValue = bossHealth.maxHP;
        bossHPSlider.value = bossHealth.currentHP;

        if (bossHPText != null)
            bossHPText.text = $"BOSS {bossHealth.currentHP}/{bossHealth.maxHP}";

        bossHealth.OnHPChanged += OnBossHPChanged;
    }
    public void HideBossHP()
    {
        if (bossHPRoot != null) bossHPRoot.SetActive(false);

        if (_boundBossHealth != null)
            _boundBossHealth.OnHPChanged -= OnBossHPChanged;

        _boundBossHealth = null;
    }

    void OnBossHPChanged(int cur, int max)
    {
        if (bossHPSlider != null) { bossHPSlider.maxValue = max; bossHPSlider.value = cur; }
        if (bossHPText != null) bossHPText.text = $"BOSS {cur}/{max}";
    }

    IEnumerator HideBossAlertAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (bossAlertRoot != null)
            bossAlertRoot.SetActive(false);
    }
}
