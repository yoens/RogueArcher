using UnityEngine;
using System.Collections;

public class FlashEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.08f;

    SpriteRenderer[] _renderers;
    Color[] _originalColors;
    bool _playing;

    void Awake()
    {
        // 본인 + 자식에 있는 모든 SpriteRenderer 수집
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
            _originalColors[i] = _renderers[i].color;
    }

    public void Flash()
    {
        if (_playing) return;
        if (_renderers == null || _renderers.Length == 0)
        {
            Debug.LogWarning($"[FlashEffect] SpriteRenderer 없음: {name}");
            return;
        }

        Debug.Log($"[FlashEffect] Flash !! on {name}");
        StartCoroutine(CoFlash());
    }

    IEnumerator CoFlash()
    {
        _playing = true;

        // 색 바꾸기
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        // 원래대로
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].color = _originalColors[i];

        _playing = false;
    }
}
