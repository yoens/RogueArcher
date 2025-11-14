using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string FilePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        if (data == null) return;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"[SaveSystem] Saved to {FilePath}\n{json}");
    }

    public static SaveData Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.Log("[SaveSystem] No save file, create new SaveData");
            return new SaveData();
        }

        string json = File.ReadAllText(FilePath);
        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) data = new SaveData();
        Debug.Log($"[SaveSystem] Loaded from {FilePath}\n{json}");
        return data;
    }

    public static void Delete()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Debug.Log("[SaveSystem] Save file deleted");
        }
    }
}
