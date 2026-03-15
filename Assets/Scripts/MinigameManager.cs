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

    void Awake()
    {
        if (fishingController == null)
            fishingController = FindObjectOfType<FishingController>();
    }

    // Called by LineController at the START of the snake phase
    // Shows a live countdown so the player knows to steer NOW
    public void BeginSteerPhase(float duration)
    {
        panel.gameObject.SetActive(true);
        // Hide background during steer phase so it's just floating text
        var bg = panel.GetComponent<UnityEngine.UI.Image>();
        if (bg != null) bg.enabled = false;
        if (hookedInfoText != null) hookedInfoText.text = "Move the bobber with WASD!";
        StartCoroutine(SteerCountdown(duration));
    }

    IEnumerator SteerCountdown(float duration)
    {
        if (countdownText != null) countdownText.enabled = true;
        float t = duration;
        while (t > 0f)
        {
            if (countdownText != null)
                countdownText.text = "STEER! " + Mathf.CeilToInt(t);
            t -= Time.deltaTime;
            yield return null;
        }
        if (countdownText != null)
        {
            countdownText.text = "REEL!";
            yield return new WaitForSeconds(0.4f);
            countdownText.enabled = false;
        }
    }

    public void StartMinigame(List<FishData> hookedFish)
    {
        currentFish = hookedFish;

        if (hookedFish.Count == 0)
        {
            bubblesNeeded = baseBubblesNeeded;
            bubbleLifetime = baseBubbleLifetime;
        }
        else
        {
            int totalDifficulty = 0;
            foreach (FishData f in hookedFish) totalDifficulty += f.difficulty;
            bubblesNeeded = baseBubblesNeeded + totalDifficulty;
            bubbleLifetime = Mathf.Max(minBubbleLifetime, baseBubbleLifetime - (totalDifficulty * lifetimeReductionPerDifficulty));
        }

        // Update hooked info text then go straight to bubbles — no second countdown
        if (hookedInfoText != null)
        {
            if (hookedFish.Count == 0)
                hookedInfoText.text = "Nothing hooked!";
            else
            {
                var names = new System.Collections.Generic.List<string>();
                foreach (FishData f in hookedFish)
                {
                    bool known = FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(f.fishName);
                    names.Add(known ? f.fishName : "???");
                }
                hookedInfoText.text = "Hooked: " + string.Join(", ", names);
            }
        }

        panel.gameObject.SetActive(true);
        // Restore background for the actual minigame
        var bg = panel.GetComponent<UnityEngine.UI.Image>();
        if (bg != null) bg.enabled = true;
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
        Debug.Log($"[Minigame] EndMinigame called. popped={popped} needed={bubblesNeeded} missed={missed}");

        if (panel != null) panel.gameObject.SetActive(false);

        if (fishingController == null)
        {
            Debug.LogError("MinigameManager: fishingController is not assigned! Run Reel Easy > 3. Setup Scene.");
            return;
        }

        if (popped >= bubblesNeeded)
        {
            var caught = currentFish ?? new System.Collections.Generic.List<FishData>();
            Debug.Log($"[Minigame] Success! Fish caught: {caught.Count}");
            fishingController.CatchFishSuccess(caught);
        }
        else
        {
            Debug.Log("[Minigame] Failed — line snapped.");
            fishingController.CatchFishFail();
        }
    }
}
