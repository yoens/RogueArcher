using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindManager : MonoBehaviour
{
    public static InputRebindManager Instance;

    [Header("���� InputActionAsset")]
    public InputActionAsset actions;   

    const string REBIND_KEY = "InputRebinds";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        
        LoadRebinds();
    }

    public void SaveRebinds()
    {
        if (actions == null) return;

        string json = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(REBIND_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("[InputRebindManager] Saved rebind json: " + json);
    }

    public void LoadRebinds()
    {
        if (actions == null) return;
        if (!PlayerPrefs.HasKey(REBIND_KEY)) return;

        string json = PlayerPrefs.GetString(REBIND_KEY);
        actions.LoadBindingOverridesFromJson(json);

        Debug.Log("[InputRebindManager] Loaded rebind json: " + json);
    }

    public void ClearRebinds()
    {
        if (actions == null) return;

        actions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(REBIND_KEY);
        Debug.Log("[InputRebindManager] Cleared rebinds");
    }
}
