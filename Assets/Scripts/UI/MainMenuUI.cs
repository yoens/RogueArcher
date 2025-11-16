using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Slot Info Texts")]
    public TextMeshProUGUI[] slotLabels;   // 슬롯 0,1,2에 대응하는 텍스트들

    [Header("Difficulty Buttons (Optional)")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    [Header("Start Button")]
    public Button startButton;

    int _selectedSlot = 0;
    Difficulty _selectedDifficulty = Difficulty.Normal;

    void Start()
    {
        // 기본값
        _selectedSlot = 0;
        _selectedDifficulty = Difficulty.Normal;

        // 슬롯 라벨 갱신
        RefreshSlotLabels();
        RefreshDifficultyButtons();
    }

    // --- 슬롯 선택 (UI Button에서 호출) ---
    public void OnSelectSlot(int slotIndex)
    {
        _selectedSlot = slotIndex;
        Debug.Log($"[MainMenu] Slot {_selectedSlot} selected");
        RefreshSlotLabels();
    }

    // --- 난이도 선택 (UI Button에서 호출) ---
    public void OnSelectDifficulty(int diff)
    {
        _selectedDifficulty = (Difficulty)diff;
        Debug.Log($"[MainMenu] Difficulty = {_selectedDifficulty}");
        RefreshDifficultyButtons();
    }

    // --- Start Game 버튼 ---
    public void OnClickStartGame()
    {
        // 선택한 슬롯/난이도를 전역 설정
        RunConfig.SaveSlotIndex = _selectedSlot;
        RunConfig.Difficulty = _selectedDifficulty;

        // 게임 씬 로드
        SceneManager.LoadScene("Game");
    }

    void RefreshSlotLabels()
    {
        // 슬롯 라벨에 각 슬롯 세이브 데이터 표시
        for (int i = 0; i < slotLabels.Length; i++)
        {
            var label = slotLabels[i];
            if (label == null) continue;

            SaveData data = SaveSystem.Load(i);
            string title = (i == _selectedSlot) ? $"[Slot {i}]" : $"Slot {i}";
            label.text = $"{title}\nBest: {data.bestScore}\nRuns: {data.totalRuns}\nLastDiff: {data.lastDifficulty}";
        }
    }

    void RefreshDifficultyButtons()
    {
        // 선택된 난이도에 따라 버튼 색 등을 바꿔줄 수 있음
        // (일단 interactable만 예시로)
        if (easyButton != null)
            easyButton.interactable = _selectedDifficulty != Difficulty.Easy;
        if (normalButton != null)
            normalButton.interactable = _selectedDifficulty != Difficulty.Normal;
        if (hardButton != null)
            hardButton.interactable = _selectedDifficulty != Difficulty.Hard;
    }
}
