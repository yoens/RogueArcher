using UnityEngine;

public static class SaveSystem
{
    const string KEY_PREFIX = "SaveSlot_";

    public static void Save(SaveData data, int slotIndex = 0)
    {
        if (data == null) data = new SaveData();

        string json = JsonUtility.ToJson(data);
        string key = KEY_PREFIX + slotIndex;

        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
        Debug.Log($"[SaveSystem] Saved slot {slotIndex}: {json}");
    }

    public static SaveData Load(int slotIndex = 0)
    {
        string key = KEY_PREFIX + slotIndex;

        if (!PlayerPrefs.HasKey(key))
        {
            Debug.Log($"[SaveSystem] No data for slot {slotIndex}, create new");
            return new SaveData();
        }

        string json = PlayerPrefs.GetString(key);
        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) data = new SaveData();

        Debug.Log($"[SaveSystem] Loaded slot {slotIndex}: {json}");
        return data;
    }

    //슬롯 데이터 초기화(리셋)
    public static void DeleteSlot(int slotIndex = 0)
    {
        string key = KEY_PREFIX + slotIndex;

        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] Deleted slot {slotIndex}");
        }
        else
        {
            Debug.Log($"[SaveSystem] No data to delete for slot {slotIndex}");
        }
    }

    // (선택) 슬롯이 존재하는지 체크할 때 쓰기 좋음
    public static bool HasSlot(int slotIndex = 0)
    {
        string key = KEY_PREFIX + slotIndex;
        return PlayerPrefs.HasKey(key);
    }
}
