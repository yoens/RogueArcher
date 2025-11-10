using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;

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
}
