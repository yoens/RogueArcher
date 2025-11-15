using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NamedClip
{
    public string key;      // "BGM_Stage", "SFX_Shoot" 같은 키
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public NamedClip[] bgmClips;

    [Header("SFX Clips")]
    public NamedClip[] sfxClips;

    Dictionary<string, AudioClip> _bgmMap;
    Dictionary<string, AudioClip> _sfxMap;

    void Awake()
    {
        // 싱글톤 세팅
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitDictionaries()
    {
        _bgmMap = new Dictionary<string, AudioClip>();
        _sfxMap = new Dictionary<string, AudioClip>();

        foreach (var nc in bgmClips)
        {
            if (nc.clip != null && !string.IsNullOrEmpty(nc.key))
                _bgmMap[nc.key] = nc.clip;
        }

        foreach (var nc in sfxClips)
        {
            if (nc.clip != null && !string.IsNullOrEmpty(nc.key))
                _sfxMap[nc.key] = nc.clip;
        }
    }

    // ========== BGM ==========

    public void PlayBGM(string key, bool loop = true)
    {
        if (bgmSource == null) return;

        if (!_bgmMap.TryGetValue(key, out var clip) || clip == null)
        {
            Debug.LogWarning($"[AudioManager] BGM key not found: {key}");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return; // 같은 곡이면 다시 안 틂

        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    // ========== SFX ==========

    public void PlaySFX(string key, float volume = 1f)
    {
        if (sfxSource == null) return;

        if (!_sfxMap.TryGetValue(key, out var clip) || clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX key not found: {key}");
            return;
        }

        float originalPitch = sfxSource.pitch;

        // 피격 사운드만 빠르게
        if (key == "SFX_HitEnemy")
            sfxSource.pitch = 1.25f;
        else
            sfxSource.pitch = 1f;

        sfxSource.PlayOneShot(clip, volume);

        sfxSource.pitch = originalPitch;
    }
}
