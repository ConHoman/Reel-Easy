using System.Collections.Generic;
using UnityEngine;

// Tracks which fish the player has caught at least once.
// Persists across runs via PlayerPrefs so the journal fills up over time.
public class FishJournal : MonoBehaviour
{
    public static FishJournal Instance;

    private HashSet<string> discovered        = new HashSet<string>();
    private HashSet<string> discoveredFlavors = new HashSet<string>();
    private const string SaveKey       = "FishJournal_v1";
    private const string FlavorSaveKey = "FishFlavors_v1";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
        LoadFlavors();
    }

    // Called at startup by any script that needs the journal to exist
    public static void EnsureExists()
    {
        if (Instance == null)
            new GameObject("FishJournal").AddComponent<FishJournal>();
    }

    public bool IsDiscovered(string fishName) => discovered.Contains(fishName);

    public void Discover(string fishName)
    {
        if (discovered.Add(fishName))
            Save();
    }

    static string FlavorKey(string fishName, FishFlavor flavor) => fishName + "|" + (int)flavor;

    public bool IsDiscoveredFlavor(string fishName, FishFlavor flavor) =>
        discoveredFlavors.Contains(FlavorKey(fishName, flavor));

    public void DiscoverFlavor(string fishName, FishFlavor flavor)
    {
        if (discoveredFlavors.Add(FlavorKey(fishName, flavor)))
            SaveFlavors();
    }

    void Save()
    {
        PlayerPrefs.SetString(SaveKey, string.Join(",", discovered));
        PlayerPrefs.Save();
    }

    void Load()
    {
        string saved = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(saved)) return;
        foreach (string name in saved.Split(','))
            if (!string.IsNullOrEmpty(name)) discovered.Add(name);
    }

    void SaveFlavors()
    {
        PlayerPrefs.SetString(FlavorSaveKey, string.Join(",", discoveredFlavors));
        PlayerPrefs.Save();
    }

    void LoadFlavors()
    {
        string saved = PlayerPrefs.GetString(FlavorSaveKey, "");
        if (string.IsNullOrEmpty(saved)) return;
        foreach (string entry in saved.Split(','))
            if (!string.IsNullOrEmpty(entry)) discoveredFlavors.Add(entry);
    }
}
