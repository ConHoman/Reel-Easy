using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Replaces BubbleGameManager.
// Scales bubble count and speed based on the difficulty of hooked fish.
public class MinigameManager : MonoBehaviour
{
    [Header("References")]
    public FishingController fishingController;
    public GameObject bubblePrefab;
    public RectTransform panel;
    public TMP_Text countdownText;
    public TMP_Text hookedInfoText; // optional: shows what fish are hooked

    [Header("Base Difficulty")]
    public int baseBubblesNeeded = 2;
    public int allowedMisses = 3;
    public float baseBubbleLifetime = 1.2f;
    public float lifetimeReductionPerDifficulty = 0.08f;
    public float minBubbleLifetime = 0.5f;

    private int bubblesNeeded;
    private float bubbleLifetime;
    private int popped;
    private int missed;
    private List<FishData> currentFish;

    public void StartMinigame(List<FishData> hookedFish)
    {
        currentFish = hookedFish;

        if (hookedFish.Count == 0)
        {
            // Nothing hooked — start a basic minigame anyway so casting always does something
            bubblesNeeded = baseBubblesNeeded;
            bubbleLifetime = baseBubbleLifetime;
            StartCoroutine(StartSequence());
            return;
        }

        int totalDifficulty = 0;
        foreach (FishData f in hookedFish)
            totalDifficulty += f.difficulty;

        bubblesNeeded = baseBubblesNeeded + totalDifficulty;
        bubbleLifetime = Mathf.Max(minBubbleLifetime, baseBubbleLifetime - (totalDifficulty * lifetimeReductionPerDifficulty));

        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        panel.gameObject.SetActive(true);

        if (hookedInfoText != null)
        {
            string names = "";
            foreach (FishData f in currentFish)
                names += f.fishName + " ";
            hookedInfoText.text = "Hooked: " + names.Trim();
        }

        countdownText.enabled = true;

        countdownText.text = "3..";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2..";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1..";
        yield return new WaitForSeconds(1f);
        countdownText.text = "REEL!";
        yield return new WaitForSeconds(0.5f);

        countdownText.enabled = false;

        popped = 0;
        missed = 0;

        StartCoroutine(BubbleLoop());
    }

    IEnumerator BubbleLoop()
    {
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);

        float panelW = panel.rect.width;
        float panelH = panel.rect.height;

        while (popped < bubblesNeeded && missed < allowedMisses)
        {
            GameObject bubble = Instantiate(bubblePrefab, panel);
            RectTransform rt = bubble.GetComponent<RectTransform>();

            float halfW = rt.rect.width * 0.5f;
            float halfH = rt.rect.height * 0.5f;

            float x = Random.Range(-panelW / 2 + halfW, panelW / 2 - halfW);
            float y = Random.Range(-panelH / 2 + halfH, panelH / 2 - halfH);
            rt.anchoredPosition = new Vector2(x, y);

            bool poppedThisBubble = false;
            float life = bubbleLifetime;

            Button clicker = bubble.AddComponent<Button>();
            clicker.transition = Selectable.Transition.None;
            clicker.onClick.AddListener(() =>
            {
                poppedThisBubble = true;
                popped++;
                Destroy(bubble);
            });

            while (life > 0f && !poppedThisBubble)
            {
                life -= Time.deltaTime;
                yield return null;
            }

            if (!poppedThisBubble)
            {
                missed++;
                if (bubble != null) Destroy(bubble);
            }

            yield return new WaitForSeconds(0.1f);
        }

        EndMinigame();
    }

    void EndMinigame()
    {
        panel.gameObject.SetActive(false);

        if (popped >= bubblesNeeded)
            fishingController.CatchFishSuccess(currentFish ?? new System.Collections.Generic.List<FishData>());
        else
            fishingController.CatchFishFail();
    }
}
