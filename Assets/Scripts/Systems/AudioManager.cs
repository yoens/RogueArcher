using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioClip hitClip;
    public AudioClip explosionClip;
    AudioSource _src;

    void Awake()
    {
        Instance = this;
        _src = GetComponent<AudioSource>();
    }

    public void PlayHit() => _src?.PlayOneShot(hitClip);
    public void PlayExplosion() => _src?.PlayOneShot(explosionClip);
}
