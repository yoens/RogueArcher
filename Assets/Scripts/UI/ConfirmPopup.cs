using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;

    System.Action _onYes;
    System.Action _onNo;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(string msg, System.Action onYes, System.Action onNo = null)
    {
        gameObject.SetActive(true);
        messageText.text = msg;

        _onYes = onYes;
        _onNo = onNo;

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            _onYes?.Invoke();
            gameObject.SetActive(false);
        });

        noButton.onClick.AddListener(() =>
        {
            _onNo?.Invoke();
            gameObject.SetActive(false);
        });
    }
}
