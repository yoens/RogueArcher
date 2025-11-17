using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;  // 사운드 + 키 설정 패널

    void Start()
    {
        // 시작할 때 옵션 패널은 꺼두기
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // 홈 씬에서는 시간 멈출 필요 X
        Time.timeScale = 1f;

        // BGM 홈 전용으로 틀고 싶으면
        AudioManager.Instance?.PlayBGM("BGM_Menu");
    }

    // Game 버튼
    public void OnClickGame()
    {
        // 슬롯 선택 있는 메인 메뉴 씬 이름으로 변경
        SceneManager.LoadScene("MainMenu");
    }

    // Settings 버튼
    public void OnClickSettings()
    {
        if (settingsPanel == null) return;

        bool active = !settingsPanel.activeSelf;
        settingsPanel.SetActive(active);

        // 홈 화면은 애초에 게임 진행 중이 아니라서 굳이 Time.timeScale 조정 안 해도 됨
    }
    public void OnClickCloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // Quit (PC 빌드용)
    public void OnClickQuit()
    {
        Application.Quit();
    }
}
