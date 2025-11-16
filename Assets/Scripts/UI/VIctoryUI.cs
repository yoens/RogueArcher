using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    public GameObject root;

    void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}