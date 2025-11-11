using UnityEngine;
using TMPro;
using System.Collections;

public class GameHUD : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject bossAlertRoot;
    public TextMeshProUGUI bossAlertText;

    void Awake()
    {
        // ★ 시작할 때 무조건 꺼두기
        if (bossAlertRoot != null)
            bossAlertRoot.SetActive(false);
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

    IEnumerator HideBossAlertAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (bossAlertRoot != null)
            bossAlertRoot.SetActive(false);
    }
}
