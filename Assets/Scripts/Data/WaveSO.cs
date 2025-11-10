using UnityEngine;

[System.Serializable]
public class WaveEnemyInfo
{
    public EnemySO enemy;      // 어떤 적
    public int count = 5;      // 몇 마리
    public float spawnInterval = 0.5f; // 간격
}

[CreateAssetMenu(menuName = "RogueArcher/Wave", fileName = "WaveSO_")]
public class WaveSO : ScriptableObject
{
    public string waveName = "Wave 1";
    public float startDelay = 0f;   // 웨이브 시작 전 대기
    public WaveEnemyInfo[] enemies; // 이 웨이브에 등장하는 적 묶음들
}
