using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public GameObject root;
    public Button button1;
    public Button button2;
    public Button button3;

    // 버튼에 텍스트가 따로 있다면
    public TextMeshProUGUI label1;
    public TextMeshProUGUI label2;
    public TextMeshProUGUI label3;

    Action<UpgradeSO> _onSelected;
    UpgradeSO[] _currentOptions;   // 지금 화면에 떠 있는 3개

    void Awake()
    {
        root.SetActive(false);

        button1.onClick.AddListener(() => Select(0));
        button2.onClick.AddListener(() => Select(1));
        button3.onClick.AddListener(() => Select(2));
    }

    // 랜덤으로 뽑힌 3개를 보여주는 버전
    public void Show(UpgradeSO[] options, Action<UpgradeSO> onSelected)
    {
        _currentOptions = options;
        _onSelected = onSelected;

        // 버튼 라벨 채우기
        if (label1 != null && options.Length > 0) label1.text = options[0].displayName;
        if (label2 != null && options.Length > 1) label2.text = options[1].displayName;
        if (label3 != null && options.Length > 2) label3.text = options[2].displayName;

        root.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        root.SetActive(false);
        Time.timeScale = 1f;
        _onSelected = null;
        _currentOptions = null;
    }

    void Select(int index)
    {
        if (_currentOptions != null && index < _currentOptions.Length)
        {
            _onSelected?.Invoke(_currentOptions[index]);
        }
        Hide();
    }
}
