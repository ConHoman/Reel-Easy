using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RunRecord
{
    public int score;
    public int fishCaught;
    public string bestFish;
}

public class HallOfFame : MonoBehaviour
{
    public static HallOfFame Instance;

    private const string SaveKey = "HallOfFame_v1";
    private const int MaxRecords = 5;

    void Awake()
    {
        Instance = this;
    }

    public void SaveRun(int score, int fishCaught, string bestFish)
    {
        List<RunRecord> records = LoadTopRuns();
        records.Add(new RunRecord { score = score, fishCaught = fishCaught, bestFish = bestFish });
        records.Sort((a, b) => b.score.CompareTo(a.score));
        if (records.Count > MaxRecords)
            records.RemoveRange(MaxRecords, records.Count - MaxRecords);

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new RunRecordWrapper { records = records }));
        PlayerPrefs.Save();
    }

    public List<RunRecord> LoadTopRuns()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json)) return new List<RunRecord>();
        RunRecordWrapper wrapper = JsonUtility.FromJson<RunRecordWrapper>(json);
        return wrapper?.records ?? new List<RunRecord>();
    }

    [Serializable]
    private class RunRecordWrapper
    {
        public List<RunRecord> records;
    }
}
