using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Settings")]
    public int fishRequired = 5;
    public int maxFails = 3;

    [Header("UI References")]
    public TMP_Text questText;
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;

    int fishCaught = 0;
    int fails = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateQuestText();
        gameOverPanel.SetActive(false);
    }

    public void FishCaught()
    {
        fishCaught++;
        UpdateQuestText();

        if (fishCaught >= fishRequired)
            Win();
    }

    public void FishFailed()
    {
        fails++;
        UpdateQuestText();

        if (fails >= maxFails)
            Lose();
    }

    void UpdateQuestText()
    {
        questText.text =
            "Quest: Catch " + fishRequired + " Fish\n" +
            "Caught: " + fishCaught + "/" + fishRequired + "\n" +
            "Fails: " + fails + "/" + maxFails;
    }

    void Win()
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = "QUEST COMPLETE!";
        Time.timeScale = 0f;
    }

    void Lose()
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = "GAME OVER";
        Time.timeScale = 0f;
    }

}
