using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel; 

    void Start()
    {
       
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        Time.timeScale = 1f;

        AudioManager.Instance?.PlayBGM("BGM_Menu");
    }

    public void OnClickGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickSettings()
    {
        if (settingsPanel == null) return;

        bool active = !settingsPanel.activeSelf;
        settingsPanel.SetActive(active);

    }
    public void OnClickCloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
