using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public GameObject root; // GameOverCanvas 또는 Panel
    public Button restartButton;

    void Awake()
    {
        root.SetActive(false);
        restartButton.onClick.AddListener(Restart);
    }

    public void Show()
    {
        root.SetActive(true);
        Time.timeScale = 0f; // 일시정지
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
