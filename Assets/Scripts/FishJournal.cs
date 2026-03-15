using System.Collections.Generic;
using UnityEngine;

// Tracks which fish the player has caught at least once.
// Persists across runs via PlayerPrefs so the journal fills up over time.
public class FishJournal : MonoBehaviour
{
    public static FishJournal Instance;

    private HashSet<string> discovered = new HashSet<string>();
    private const string SaveKey = "FishJournal_v1";

    void Awake()
    {
        Instance = this;
        Load();
    }

    public bool IsDiscovered(string fishName) => discovered.Contains(fishName);

    public void Discover(string fishName)
    {
        if (discovered.Add(fishName))
            Save();
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
}
