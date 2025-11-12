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

    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}