using UnityEngine;
using UnityEngine.UI;
using System;

public class UpgradeUI : MonoBehaviour
{
    public GameObject root;   // 전체 패널
    public Button button1;
    public Button button2;
    public Button button3;

    Action<int> _onSelected;

    void Awake()
    {
        root.SetActive(false);

        button1.onClick.AddListener(() => Select(0));
        button2.onClick.AddListener(() => Select(1));
        button3.onClick.AddListener(() => Select(2));
    }

    public void Show(Action<int> onSelected)
    {
        _onSelected = onSelected;
        root.SetActive(true);
        Time.timeScale = 0f; // 강화 고르는 동안 멈춤
    }

    public void Hide()
    {
        root.SetActive(false);
        Time.timeScale = 1f;
        _onSelected = null;
    }

    void Select(int index)
    {
        _onSelected?.Invoke(index);
        Hide();
    }
}
