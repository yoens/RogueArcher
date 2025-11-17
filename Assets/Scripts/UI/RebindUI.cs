using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;
using UnityEngine.UI;

public class RebindMoveUI : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference moveAction;  // Player/Move 액션 참조 (Vector2 Composite)

    // composite 안의 binding index들 (인스펙터에서 지정)
    public int upBindingIndex;
    public int downBindingIndex;
    public int leftBindingIndex;
    public int rightBindingIndex;

    public SoundSettingsUI soundSettingsUI;

    [Header("UI")]
    public Button upButton;
    public Button downButton;
    public Button leftButton;
    public Button rightButton;

    public TextMeshProUGUI upLabel;
    public TextMeshProUGUI downLabel;
    public TextMeshProUGUI leftLabel;
    public TextMeshProUGUI rightLabel;

    InputAction _action;

    void Awake()
    {
        if (moveAction != null)
            _action = moveAction.action;
    }

    void Start()
    {
        // 버튼 이벤트 연결
        if (upButton != null) upButton.onClick.AddListener(() => StartRebind(upBindingIndex, upLabel));
        if (downButton != null) downButton.onClick.AddListener(() => StartRebind(downBindingIndex, downLabel));
        if (leftButton != null) leftButton.onClick.AddListener(() => StartRebind(leftBindingIndex, leftLabel));
        if (rightButton != null) rightButton.onClick.AddListener(() => StartRebind(rightBindingIndex, rightLabel));

        RefreshDisplay();
    }

    void OnDestroy()
    {
        if (upButton != null) upButton.onClick.RemoveAllListeners();
        if (downButton != null) downButton.onClick.RemoveAllListeners();
        if (leftButton != null) leftButton.onClick.RemoveAllListeners();
        if (rightButton != null) rightButton.onClick.RemoveAllListeners();
    }

    void RefreshDisplay()
    {
        if (_action == null) return;

        if (upLabel != null)
            upLabel.text = GetBindingDisplay(upBindingIndex);
        if (downLabel != null)
            downLabel.text = GetBindingDisplay(downBindingIndex);
        if (leftLabel != null)
            leftLabel.text = GetBindingDisplay(leftBindingIndex);
        if (rightLabel != null)
            rightLabel.text = GetBindingDisplay(rightBindingIndex);
    }

    string GetBindingDisplay(int bindingIndex)
    {
        if (_action == null || bindingIndex < 0 || bindingIndex >= _action.bindings.Count)
            return "---";

        return InputControlPath.ToHumanReadableString(
            _action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    void StartRebind(int bindingIndex, TextMeshProUGUI label)
    {
        if (_action == null) return;
        if (bindingIndex < 0 || bindingIndex >= _action.bindings.Count) return;

        // UI에 표시
        if (label != null)
            label.text = "...";

        _action.Disable();

        var rebind = _action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                _action.Enable();
                op.Dispose();

                SaveRebinds();
                RefreshDisplay();
            });

        rebind.Start();
    }

    void SaveRebinds()
    {
        if (_action == null) return;

        var map = _action.actionMap;
        if (map == null) return;

        string json = map.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds_Player", json);
    }

    public void LoadRebinds()
    {
        if (_action == null) return;

        var map = _action.actionMap;
        if (map == null) return;

        string json = PlayerPrefs.GetString("Rebinds_Player", "");
        if (!string.IsNullOrEmpty(json))
        {
            map.LoadBindingOverridesFromJson(json);
        }

        RefreshDisplay();
    }
    public void OnClickOptionsButton()
    {
        if (soundSettingsUI == null)
        {
            Debug.LogWarning("[GameHUD] soundSettingsUI is null!");
            return;
        }

        Debug.Log("[GameHUD] Options button clicked");
        soundSettingsUI.TogglePanel();
    }
}
