using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    [Header("Run Settings")]
    public int startingLines = 5;

    [Header("UI References")]
    public TMP_Text runInfoText;
    public GameObject runOverPanel;
    public TMP_Text runOverText;

    [Header("Scene References")]
    public FishSpawner fishSpawner;
    public FishInventory fishInventory;

    int linesRemaining;
    int runScore;
    int fishCaught;
    string bestFishName = "None";
    int bestFishScore = 0;
    private int successfulCasts;

    void Awake()
    {
        Instance = this;

        if (fishSpawner == null)
            fishSpawner = FindObjectOfType<FishSpawner>();
        if (fishInventory == null)
            fishInventory = FindObjectOfType<FishInventory>();

        // Hide our panel immediately so it's never visible before StartRun()
        if (runOverPanel == null)
            runOverPanel = GameObject.Find("RunOverPanel");
        if (runOverPanel != null) runOverPanel.SetActive(false);

        // Hide any leftover game over panel from the old QuestManager
        GameObject oldPanel = GameObject.Find("GameOverPanel");
        if (oldPanel != null) oldPanel.SetActive(false);

        PerkManager.EnsureExists(); PerkPickerUI.EnsureExists(); SettingsManager.EnsureExists(); FishCompendium.EnsureExists(); DebugMenuManager.EnsureExists(); PerkViewerUI.EnsureExists();

        if (runInfoText != null) runInfoText.fontSize = 8f;
    }

    void Start()
    {
        StartRun();
    }

    public void StartRun()
    {
        linesRemaining = startingLines;
        runScore = 0;
        fishCaught = 0;
        bestFishName = "None";
        bestFishScore = 0;
        successfulCasts = 0;
        if (PerkManager.Instance != null) PerkManager.Instance.ResetForRun();

        if (runOverPanel != null) runOverPanel.SetActive(false);
        if (fishInventory != null) fishInventory.ResetInventory();
        if (fishSpawner != null) fishSpawner.RespawnFish();

        Time.timeScale = 1f;
        UpdateUI();
    }

    public void OnFishCaught(FishData fish)
    {
        fishCaught++;
        int score = PerkManager.Instance != null ? PerkManager.Instance.GetScoreForFish(fish) : fish.scoreValue;
        runScore += score;
        if (score > bestFishScore) { bestFishScore = score; bestFishName = fish.fishName; }
        UpdateUI();
    }

    public void LineSnapped()
    {
        if (PerkManager.Instance != null && PerkManager.Instance.TryConsumeLuckyLine()) { UpdateUI(); return; }
        int linesLost = PerkManager.Instance != null ? PerkManager.Instance.LinesLostOnSnap : 1;
        linesRemaining -= linesLost;
        UpdateUI();
        if (linesRemaining <= 0)
            EndRun();
    }

    void EndRun()
    {
        if (HallOfFame.Instance != null)
            HallOfFame.Instance.SaveRun(runScore, fishCaught, bestFishName);

        if (runOverPanel != null) runOverPanel.SetActive(true);
        if (runOverText != null)
            runOverText.text =
                "Run Over!\n" +
                "Score: " + runScore + "\n" +
                "Fish Caught: " + fishCaught + "\n" +
                "Best Catch: " + bestFishName;

        Time.timeScale = 0f;
    }

    public void NotifySuccessfulCast()
    {
        successfulCasts++;
        if (successfulCasts % 3 == 0 && PerkPickerUI.Instance != null)
            PerkPickerUI.Instance.ShowPicker();
    }

    public void AddLine()
    {
        linesRemaining++;
        UpdateUI();
    }

    public void RemoveLine()
    {
        linesRemaining = Mathf.Max(1, linesRemaining - 1);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (runInfoText != null)
            runInfoText.text = "Lines " + linesRemaining + "  |  Score " + runScore + "  |  Fish " + fishCaught;
    }
}
