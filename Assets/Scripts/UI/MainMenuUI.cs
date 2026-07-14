using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Slot Info Texts")]
    public TextMeshProUGUI[] slotLabels;   

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

    public void OnSelectSlot(int slotIndex)
    {
        _selectedSlot = slotIndex;
        Debug.Log($"[MainMenu] Slot {_selectedSlot} selected");
        RefreshSlotLabels();
    }

    public void OnClickResetSlot(int slotIndex)
    {
        SaveSystem.DeleteSlot(slotIndex);
        Debug.Log($"[MainMenu] Reset Slot {slotIndex}");
        RefreshSlotLabels();
    }

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

    public void OnSelectDifficulty(int diff)
    {
        _selectedDifficulty = (Difficulty)diff;
        Debug.Log($"[MainMenu] Difficulty = {_selectedDifficulty}");
        RefreshDifficultyButtons();
    }

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
