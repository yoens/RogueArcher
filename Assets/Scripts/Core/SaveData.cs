[System.Serializable]
public class SaveData
{
    
    public int bestScore;
    public int totalRuns;

   
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    public Difficulty lastDifficulty = Difficulty.Normal;

    public Difficulty bestScoreDifficulty;
}
