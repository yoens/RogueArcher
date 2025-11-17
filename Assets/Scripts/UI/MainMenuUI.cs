using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Slot Info Texts")]
    public TextMeshProUGUI[] slotLabels;   // 슬롯 0,1,2 텍스트

    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    [Header("Start Button")]
    public Button startButton;

    public ConfirmPopup confirmPopup;

    int _selectedSlot = 0;
    Difficulty _selectedDifficulty = Difficulty.Normal;

    void Start()
    {
        _selectedSlot = 0;
        _selectedDifficulty = Difficulty.Normal;

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

    // --- 슬롯 리셋 (UI Button에서 호출) ---
    // 슬롯별 Reset 버튼에 이 함수 + 인덱스 연결 (예: Slot0Reset 버튼 → 0)
    public void OnClickResetSlot(int slotIndex)
    {
        SaveSystem.DeleteSlot(slotIndex);
        Debug.Log($"[MainMenu] Reset Slot {slotIndex}");
        RefreshSlotLabels();
    }

    // 선택된 슬롯만 리셋하고 싶으면 이걸 버튼에 연결해도 됨
    public void OnClickResetSelectedSlot()
    {
        confirmPopup.Show(
            $"SLOT {_selectedSlot} DELET YOUR DATA?",
            onYes: () =>
            {
                SaveSystem.DeleteSlot(_selectedSlot);
                RefreshSlotLabels();
                Debug.Log($"[MainMenu] Slot {_selectedSlot} deleted");
            },
            onNo: () =>
            {
                Debug.Log("[MainMenu] Delete cancelled");
            }
        );
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
        RunConfig.SaveSlotIndex = _selectedSlot;
        RunConfig.Difficulty = _selectedDifficulty;

        SceneManager.LoadScene("Game");
    }

    void RefreshSlotLabels()
    {
        for (int i = 0; i < slotLabels.Length; i++)
        {
            var label = slotLabels[i];
            if (label == null) continue;

            string title = (i == _selectedSlot) ? $"[Slot {i}]" : $"Slot {i}";

            // 저장 여부 체크해서 Empty / 기록 표시
            if (!SaveSystem.HasSlot(i))
            {
                label.text = $"{title}\nEmpty";
            }
            else
            {
                SaveData data = SaveSystem.Load(i);
                label.text =
                    $"{title}\n" +
                    $"Best: {data.bestScore}\n" +
                    $"Runs: {data.totalRuns}\n" +
                    $"BestDiff: {data.bestScoreDifficulty}\n" +
                    $"LastDiff: {data.lastDifficulty}";
            }
        }
    }
    public void OnClickBackToHome()
    {
        SceneManager.LoadScene("Home");
    }

    void RefreshDifficultyButtons()
    {
        if (easyButton != null)
            easyButton.interactable = _selectedDifficulty != Difficulty.Easy;
        if (normalButton != null)
            normalButton.interactable = _selectedDifficulty != Difficulty.Normal;
        if (hardButton != null)
            hardButton.interactable = _selectedDifficulty != Difficulty.Hard;
    }
}
