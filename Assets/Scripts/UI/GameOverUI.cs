using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject root; // GameOverCanvas 또는 Panel
    public Button restartButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;

    void Awake()
    {
        root.SetActive(false);
        restartButton.onClick.AddListener(Restart);
    }

    public void Show()
    {
        root.SetActive(true);
        Time.timeScale = 0f; // 일시정지

        if (GameManager.Instance != null)
        {
            int cur = GameManager.Instance.GetScore();
            int best = GameManager.Instance.GetBestScore();

            if (scoreText != null)
                scoreText.text = $"Score: {cur}";
            if (bestScoreText != null)
                bestScoreText.text = $"Best: {best}";
        }

    }

    public void Hide()
    {
        root.SetActive(false);
        Time.timeScale = 1f;
    }

    void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
