using UnityEngine;

[CreateAssetMenu(menuName = "RogueArcher/Wave Set", fileName = "WaveSet")]
public class WaveSetSO : ScriptableObject
{
    public WaveSO[] waves;
}
