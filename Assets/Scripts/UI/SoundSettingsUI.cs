using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("AudioMixer")]
    public AudioMixer mixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    // ↘︎ AudioMixer에서 노출한 이름 그대로
    const string MASTER_PARAM = "MyExposedParam";
    const string BGM_PARAM = "MyExposedParam 1";
    const string SFX_PARAM = "MyExposedParam 2";

    void Start()
    {
        // 처음에는 꺼진 상태로 시작하고 싶으면
        gameObject.SetActive(false);

        float master = PlayerPrefs.GetFloat("Volume_Master", 1f);
        float bgm = PlayerPrefs.GetFloat("Volume_BGM", 1f);
        float sfx = PlayerPrefs.GetFloat("Volume_SFX", 1f);

        if (masterSlider != null)
        {
            masterSlider.value = master;
            SetMasterVolume(master);
        }
        if (bgmSlider != null)
        {
            bgmSlider.value = bgm;
            SetBGMVolume(bgm);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = sfx;
            SetSFXVolume(sfx);
        }

        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameObject.activeSelf)
                TogglePanel();
        }
    }

    void OnDestroy()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMSliderChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    // HUD에서 호출할 함수
    public void TogglePanel()
    {
        bool toActive = !gameObject.activeSelf;
        Debug.Log($"[SoundSettingsUI] TogglePanel -> {toActive}");
        gameObject.SetActive(toActive);
    }

    // 패널 켜질 때 / 꺼질 때 일시정지 제어
    void OnEnable()
    {
        Debug.Log("[SoundSettingsUI] OnEnable -> Pause");
        Time.timeScale = 0f;
    }

    void OnDisable()
    {
        Debug.Log("[SoundSettingsUI] OnDisable -> Resume");
        Time.timeScale = 1f;
    }

    // --- 슬라이더 콜백 ---
    void OnMasterSliderChanged(float value)
    {
        SetMasterVolume(value);
        PlayerPrefs.SetFloat("Volume_Master", value);
    }

    void OnBGMSliderChanged(float value)
    {
        SetBGMVolume(value);
        PlayerPrefs.SetFloat("Volume_BGM", value);
    }

    void OnSFXSliderChanged(float value)
    {
        SetSFXVolume(value);
        PlayerPrefs.SetFloat("Volume_SFX", value);
    }

    // --- Mixer 적용 ---
    void SetMasterVolume(float value) => SetVolume(MASTER_PARAM, value);
    void SetBGMVolume(float value) => SetVolume(BGM_PARAM, value);
    void SetSFXVolume(float value) => SetVolume(SFX_PARAM, value);

    void SetVolume(string paramName, float value)
    {
        float v = Mathf.Clamp(value, 0.0001f, 1f);
        float dB = Mathf.Log10(v) * 20f;
        mixer.SetFloat(paramName, dB);
    }
}
