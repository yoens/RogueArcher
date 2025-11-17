using UnityEngine;

public class MainMenuBGM : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance?.PlayBGM("BGM_Menu");
    }
}
