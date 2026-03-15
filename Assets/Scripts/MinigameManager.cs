using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Handles all fishing minigames. Picks one randomly each catch.
public class MinigameManager : MonoBehaviour
{
    [Header("References")]
    public FishingController fishingController;
    public GameObject bubblePrefab;
    public RectTransform panel;
    public TMP_Text countdownText;
    public TMP_Text hookedInfoText;
    public TMP_Text hintText;

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
    private int currentAllowedMisses;
    private List<FishData> currentFish;
    private GameObject minigameContent;

    private enum MinigameType { BubblePop, TimingBar, ButtonMash, HoldZone, TugOfWar, RingDodge }

    void Awake()
    {
        if (fishingController == null)
            fishingController = FindObjectOfType<FishingController>();
        FishJournal.EnsureExists();
        SettingsManager.EnsureExists();
        if (panel != null)
            panel.SetParent(UICanvas.Get().transform, false);
    }

    // Called by LineController at the START of the snake phase
    public void BeginSteerPhase(float duration)
    {
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            var bg = panel.GetComponent<Image>();
            if (bg != null) bg.enabled = false;
        }
        if (countdownText != null) countdownText.enabled = false;
        if (hookedInfoText != null)
            hookedInfoText.text = SettingsManager.ShowHints ? "Move the bobber with WASD!" : "";

        if (hintText == null)
        {
            Canvas canvas = UICanvas.Get();
            if (canvas != null)
            {
                var hintGO = new GameObject("HintText");
                hintGO.transform.SetParent(canvas.transform, false);
                hintText = hintGO.AddComponent<TextMeshProUGUI>();
                hintText.fontSize = 11f;
                hintText.alignment = TextAlignmentOptions.Center;
                hintText.color = Color.white;
                var rt = hintGO.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.2f, 0f);
                rt.anchorMax = new Vector2(0.8f, 0.12f);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
        }

        StartCoroutine(SteerCountdown(duration));
    }

    IEnumerator SteerCountdown(float duration)
    {
        float t = duration;
        while (t > 0f)
        {
            if (hintText != null)
            {
                hintText.enabled = SettingsManager.ShowHints;
                if (SettingsManager.ShowHints) hintText.text = "STEER  " + Mathf.CeilToInt(t);
            }
            t -= Time.deltaTime;
            yield return null;
        }
        if (hintText != null && SettingsManager.ShowHints)
        {
            hintText.text = "REEL!";
            hintText.enabled = true;
            yield return new WaitForSeconds(0.4f);
        }
        if (hintText != null) hintText.enabled = false;
    }

    public void StartMinigame(List<FishData> hookedFish)
    {
        currentFish = hookedFish;

        if (hookedFish.Count == 0)
        {
            // Nothing hooked — skip minigame entirely, count as a successful cast
            if (fishingController != null)
                fishingController.CatchFishSuccess(hookedFish);
            return;
        }

        int totalDifficulty = 0;
        if (hookedFish.Count == 0)
        {
            bubblesNeeded = baseBubblesNeeded;
            bubbleLifetime = baseBubbleLifetime;
        }
        else
        {
            foreach (FishData f in hookedFish) totalDifficulty += f.difficulty;
            bubblesNeeded = baseBubblesNeeded + totalDifficulty;
            bubbleLifetime = Mathf.Max(minBubbleLifetime, baseBubbleLifetime - totalDifficulty * lifetimeReductionPerDifficulty);
        }

        if (PerkManager.Instance != null)
        {
            bubblesNeeded += PerkManager.Instance.ExtraBubblesPerFish * hookedFish.Count;
            bubblesNeeded = Mathf.Max(bubblesNeeded, PerkManager.Instance.MinBubbles);
            currentAllowedMisses = allowedMisses + PerkManager.Instance.AllowedMissesBonus;
        }
        else currentAllowedMisses = allowedMisses;

        if (hookedInfoText != null)
        {
            hookedInfoText.fontSize = 9f;
            if (hookedFish.Count == 0)
            {
                hookedInfoText.text = "Nothing hooked!";
            }
            else
            {
                var names = new List<string>();
                foreach (FishData f in hookedFish)
                {
                    bool known = FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(f.fishName);
                    names.Add(known ? f.fishName : "???");
                }
                hookedInfoText.text = "Hooked: " + string.Join(", ", names);
            }
        }

        panel.gameObject.SetActive(true);
        var bgImg = panel.GetComponent<Image>();
        if (bgImg != null) bgImg.enabled = true;
        popped = 0;
        missed = 0;

        MinigameType type = (MinigameType)Random.Range(0, System.Enum.GetValues(typeof(MinigameType)).Length);
        switch (type)
        {
            case MinigameType.BubblePop:  StartCoroutine(BubbleLoop()); break;
            case MinigameType.TimingBar:  StartCoroutine(TimingBarGame(totalDifficulty)); break;
            case MinigameType.ButtonMash: StartCoroutine(ButtonMashGame(totalDifficulty)); break;
            case MinigameType.HoldZone:   StartCoroutine(HoldZoneGame(totalDifficulty)); break;
            case MinigameType.TugOfWar:   StartCoroutine(TugOfWarGame(totalDifficulty)); break;
            case MinigameType.RingDodge:  StartCoroutine(RingDodgeGame(totalDifficulty)); break;
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    RectTransform GetFreshContent()
    {
        if (minigameContent != null) { Destroy(minigameContent); minigameContent = null; }

        // Reset panel to fill the UICanvas
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.pivot = new Vector2(0.5f, 0.5f);

        minigameContent = new GameObject("MinigameContent");
        minigameContent.transform.SetParent(panel, false);
        var rt = minigameContent.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.12f);
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    static RectTransform MakeRect(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    static TMP_Text MakeLabel(Transform parent, string text, float size, Color color)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    void EndMinigame(bool success)
    {
        Debug.Log($"[Minigame] EndMinigame success={success}");
        if (panel != null) panel.gameObject.SetActive(false);
        if (fishingController == null)
        {
            Debug.LogError("MinigameManager: fishingController not assigned!");
            return;
        }
        if (success)
            fishingController.CatchFishSuccess(currentFish ?? new List<FishData>());
        else
            fishingController.CatchFishFail();
    }

    // ── Bubble Pop ─────────────────────────────────────────────────────────────

    IEnumerator BubbleLoop()
    {
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.pivot = new Vector2(0.5f, 0.5f);

        // Wait one frame so layout updates and rect reflects actual screen size
        yield return null;

        float panelW = panel.rect.width;
        float panelH = panel.rect.height;

        while (popped < bubblesNeeded && missed < currentAllowedMisses)
        {
            GameObject bubble = Instantiate(bubblePrefab, panel);
            RectTransform rt = bubble.GetComponent<RectTransform>();

            float size = Random.Range(10f, 28f);
            rt.sizeDelta = new Vector2(size, size);

            float half = size * 0.5f;
            rt.anchoredPosition = new Vector2(
                Random.Range(-panelW / 2 + half, panelW / 2 - half),
                Random.Range(-panelH / 2 + half, panelH / 2 - half));

            bool poppedThis = false;
            float life = bubbleLifetime;
            var btn = bubble.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { poppedThis = true; popped++; Destroy(bubble); });

            while (life > 0f && !poppedThis) { life -= Time.deltaTime; yield return null; }
            if (!poppedThis) { missed++; if (bubble != null) Destroy(bubble); }

            yield return new WaitForSeconds(0.1f);
        }

        EndMinigame(popped >= bubblesNeeded);
    }

    // ── Timing Bar ─────────────────────────────────────────────────────────────
    // Press SPACE when the moving cursor is inside the green zone.

    IEnumerator TimingBarGame(int difficulty)
    {
        int hitsNeeded = 2 + difficulty + Random.Range(0, 2);
        int missesAllowed = 2 + (PerkManager.Instance != null ? PerkManager.Instance.AllowedMissesBonus : 0);
        float perkSpeedMult = PerkManager.Instance != null ? PerkManager.Instance.TimingSpeedMultiplier : 1f;
        float perkZoneMult  = PerkManager.Instance != null ? PerkManager.Instance.TimingZoneMultiplier  : 1f;
        float speed = Mathf.Min(1.4f, (0.35f + difficulty * 0.07f + Random.Range(-0.05f, 0.1f)) * perkSpeedMult);
        float zoneHalf = Mathf.Max(0.07f, (0.28f - difficulty * 0.025f + Random.Range(-0.03f, 0.03f)) * perkZoneMult);
        float zoneCenter = 0.5f + Random.Range(-0.1f, 0.1f);

        var content = GetFreshContent();

        var title = MakeLabel(content, "Press SPACE in the green zone!", 10f, Color.white);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.76f);
        titleRT.anchorMax = new Vector2(0.95f, 0.94f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        var scoreLabel = MakeLabel(content, "", 9f, Color.white);
        var scoreRT = scoreLabel.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0.05f, 0.59f);
        scoreRT.anchorMax = new Vector2(0.95f, 0.74f);
        scoreRT.offsetMin = scoreRT.offsetMax = Vector2.zero;

        var barRT = MakeRect(content, "Bar", new Color(0.15f, 0.15f, 0.15f));
        barRT.anchorMin = new Vector2(0.05f, 0.36f);
        barRT.anchorMax = new Vector2(0.95f, 0.54f);
        barRT.offsetMin = barRT.offsetMax = Vector2.zero;

        var zoneRT = MakeRect(barRT, "Zone", new Color(0.2f, 0.85f, 0.3f, 0.7f));
        zoneRT.anchorMin = new Vector2(zoneCenter - zoneHalf, 0f);
        zoneRT.anchorMax = new Vector2(zoneCenter + zoneHalf, 1f);
        zoneRT.offsetMin = zoneRT.offsetMax = Vector2.zero;

        var cursorRT = MakeRect(barRT, "Cursor", Color.white);
        cursorRT.pivot = new Vector2(0.5f, 0.5f);
        cursorRT.sizeDelta = new Vector2(5f, 0f);

        int hits = 0, misses = 0;
        float pos = Random.Range(0f, 1f), dir = Random.value > 0.5f ? 1f : -1f;

        while (hits < hitsNeeded && misses <= missesAllowed)
        {
            pos += dir * speed * Time.deltaTime;
            if (pos >= 1f) { pos = 1f; dir = -1f; }
            if (pos <= 0f) { pos = 0f; dir = 1f; }

            cursorRT.anchorMin = new Vector2(pos, 0.1f);
            cursorRT.anchorMax = new Vector2(pos, 0.9f);

            scoreLabel.text = "hits: " + hits + " / " + hitsNeeded + "   misses: " + misses + " / " + missesAllowed;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (pos >= zoneCenter - zoneHalf && pos <= zoneCenter + zoneHalf) hits++;
                else misses++;
            }

            yield return null;
        }

        Destroy(minigameContent); minigameContent = null;
        EndMinigame(hits >= hitsNeeded);
    }

    // ── Button Mash ────────────────────────────────────────────────────────────
    // Spam SPACE to fill the meter before the timer runs out.

    IEnumerator ButtonMashGame(int difficulty)
    {
        int clickMult  = PerkManager.Instance != null ? PerkManager.Instance.MashClickMultiplier     : 1;
        float timeMult = PerkManager.Instance != null ? PerkManager.Instance.MashTimeLimitMultiplier  : 1f;
        int needed = 10 + difficulty * 2 + Random.Range(-2, 3);
        float timeLimit = Mathf.Max(3f, (8f - difficulty * 0.5f + Random.Range(-0.5f, 0.5f)) * timeMult);

        var content = GetFreshContent();

        var title = MakeLabel(content, "MASH SPACE!", 14f, new Color(1f, 0.9f, 0.2f));
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.1f, 0.74f);
        titleRT.anchorMax = new Vector2(0.9f, 0.94f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // Progress bar
        var progBGRT = MakeRect(content, "ProgBG", new Color(0.15f, 0.15f, 0.15f));
        progBGRT.anchorMin = new Vector2(0.05f, 0.5f);
        progBGRT.anchorMax = new Vector2(0.95f, 0.65f);
        progBGRT.offsetMin = progBGRT.offsetMax = Vector2.zero;

        var progFillRT = MakeRect(progBGRT, "Fill", new Color(0.2f, 0.7f, 1f));
        progFillRT.anchorMin = Vector2.zero;
        progFillRT.anchorMax = new Vector2(0f, 1f);
        progFillRT.offsetMin = progFillRT.offsetMax = Vector2.zero;

        // Timer bar
        var timeBGRT = MakeRect(content, "TimeBG", new Color(0.15f, 0.15f, 0.15f));
        timeBGRT.anchorMin = new Vector2(0.05f, 0.33f);
        timeBGRT.anchorMax = new Vector2(0.95f, 0.44f);
        timeBGRT.offsetMin = timeBGRT.offsetMax = Vector2.zero;

        var timeFillRT = MakeRect(timeBGRT, "Fill", new Color(1f, 0.5f, 0.15f));
        timeFillRT.anchorMin = Vector2.zero;
        timeFillRT.anchorMax = Vector2.one;
        timeFillRT.offsetMin = timeFillRT.offsetMax = Vector2.zero;

        var counter = MakeLabel(content, "", 9f, Color.white);
        var counterRT = counter.GetComponent<RectTransform>();
        counterRT.anchorMin = new Vector2(0.1f, 0.16f);
        counterRT.anchorMax = new Vector2(0.9f, 0.3f);
        counterRT.offsetMin = counterRT.offsetMax = Vector2.zero;

        int clicks = 0;
        float elapsed = 0f;

        while (elapsed < timeLimit && clicks < needed)
        {
            elapsed += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space)) clicks += clickMult;

            progFillRT.anchorMax = new Vector2((float)clicks / needed, 1f);
            timeFillRT.anchorMax = new Vector2(1f - elapsed / timeLimit, 1f);
            counter.text = clicks + " / " + needed;

            yield return null;
        }

        Destroy(minigameContent); minigameContent = null;
        EndMinigame(clicks >= needed);
    }

    // ── Hold Zone ──────────────────────────────────────────────────────────────
    // Hold SPACE while the moving green zone is over the fixed center marker.

    IEnumerator HoldZoneGame(int difficulty)
    {
        bool noDrain      = PerkManager.Instance != null && PerkManager.Instance.HoldZoneNoDrain;
        float holdMult    = PerkManager.Instance != null ? PerkManager.Instance.HoldTimeMultiplier : 1f;
        float holdNeeded  = (1.5f + difficulty * 0.35f + Random.Range(-0.3f, 0.3f)) * holdMult;
        float speed = Mathf.Min(1.4f, 0.3f + difficulty * 0.08f + Random.Range(-0.05f, 0.1f));
        float zoneHalf = Mathf.Max(0.08f, 0.28f - difficulty * 0.02f + Random.Range(-0.02f, 0.02f));
        float timeLimit = holdNeeded * 5f + 4f;

        var content = GetFreshContent();

        var title = MakeLabel(content, "Hold SPACE while zone covers the marker!", 9f, Color.white);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.02f, 0.77f);
        titleRT.anchorMax = new Vector2(0.98f, 0.95f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // Bar
        var barRT = MakeRect(content, "Bar", new Color(0.15f, 0.15f, 0.15f));
        barRT.anchorMin = new Vector2(0.05f, 0.52f);
        barRT.anchorMax = new Vector2(0.95f, 0.67f);
        barRT.offsetMin = barRT.offsetMax = Vector2.zero;

        // Moving green zone
        var zoneRT = MakeRect(barRT, "Zone", new Color(0.2f, 0.85f, 0.3f, 0.6f));
        zoneRT.anchorMin = new Vector2(0.5f - zoneHalf, 0f);
        zoneRT.anchorMax = new Vector2(0.5f + zoneHalf, 1f);
        zoneRT.offsetMin = zoneRT.offsetMax = Vector2.zero;

        // Fixed center marker
        var markerRT = MakeRect(barRT, "Marker", Color.white);
        markerRT.anchorMin = new Vector2(0.5f, 0.1f);
        markerRT.anchorMax = new Vector2(0.5f, 0.9f);
        markerRT.pivot = new Vector2(0.5f, 0.5f);
        markerRT.sizeDelta = new Vector2(4f, 0f);

        // Hold progress bar
        var holdBGRT = MakeRect(content, "HoldBG", new Color(0.15f, 0.15f, 0.15f));
        holdBGRT.anchorMin = new Vector2(0.05f, 0.35f);
        holdBGRT.anchorMax = new Vector2(0.95f, 0.46f);
        holdBGRT.offsetMin = holdBGRT.offsetMax = Vector2.zero;

        var holdFillRT = MakeRect(holdBGRT, "Fill", new Color(0.9f, 0.7f, 0.1f));
        holdFillRT.anchorMin = Vector2.zero;
        holdFillRT.anchorMax = new Vector2(0f, 1f);
        holdFillRT.offsetMin = holdFillRT.offsetMax = Vector2.zero;

        var status = MakeLabel(content, "", 9f, Color.white);
        var statusRT = status.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0.1f, 0.17f);
        statusRT.anchorMax = new Vector2(0.9f, 0.31f);
        statusRT.offsetMin = statusRT.offsetMax = Vector2.zero;

        float zonePos = Random.Range(0.2f, 0.8f), zoneDir = Random.value > 0.5f ? 1f : -1f;
        float holdAccum = 0f, elapsed = 0f;

        while (holdAccum < holdNeeded && elapsed < timeLimit)
        {
            elapsed += Time.deltaTime;

            zonePos += zoneDir * speed * Time.deltaTime;
            if (zonePos + zoneHalf >= 1f) { zonePos = 1f - zoneHalf; zoneDir = -1f; }
            if (zonePos - zoneHalf <= 0f) { zonePos = zoneHalf; zoneDir = 1f; }

            bool inZone = Mathf.Abs(zonePos - 0.5f) <= zoneHalf;
            bool holding = Input.GetKey(KeyCode.Space);
            if (inZone && holding)
                holdAccum += Time.deltaTime;
            else if (!inZone && holding && !noDrain)
                holdAccum = Mathf.Max(0f, holdAccum - Time.deltaTime * 0.6f);

            zoneRT.anchorMin = new Vector2(zonePos - zoneHalf, 0f);
            zoneRT.anchorMax = new Vector2(zonePos + zoneHalf, 1f);
            holdFillRT.anchorMax = new Vector2(holdAccum / holdNeeded, 1f);

            if (inZone)
                status.text = holding ? "Holding!  " + holdAccum.ToString("F1") + " / " + holdNeeded.ToString("F1") + "s" : "Press SPACE!";
            else
                status.text = (holding && !noDrain) ? "Let go! Losing progress..." : "Wait for the zone...";

            yield return null;
        }

        Destroy(minigameContent); minigameContent = null;
        EndMinigame(holdAccum >= holdNeeded);
    }

    // ── Tug of War ─────────────────────────────────────────────────────────────
    // Spam SPACE to pull the indicator left. The fish pulls it right. Win before it escapes.

    IEnumerator TugOfWarGame(int difficulty)
    {
        float playerMult = PerkManager.Instance != null ? PerkManager.Instance.TugPlayerMultiplier : 1f;
        float fishMult   = PerkManager.Instance != null ? PerkManager.Instance.TugFishMultiplier   : 1f;
        float fishPull   = (0.08f + difficulty * 0.015f + Random.Range(-0.01f, 0.015f)) * fishMult;
        float playerPull = (0.06f + Random.Range(-0.005f, 0.005f)) * playerMult;
        float surgeTimer = 0f;
        float surgeDuration = 0f;
        float surgeInterval = Mathf.Max(2f, 5f - difficulty * 0.4f);

        var content = GetFreshContent();

        var title = MakeLabel(content, "Spam SPACE to reel it in!", 10f, Color.white);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.78f);
        titleRT.anchorMax = new Vector2(0.95f, 0.95f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // Track background
        var trackRT = MakeRect(content, "Track", new Color(0.15f, 0.15f, 0.15f));
        trackRT.anchorMin = new Vector2(0.05f, 0.48f);
        trackRT.anchorMax = new Vector2(0.95f, 0.63f);
        trackRT.offsetMin = trackRT.offsetMax = Vector2.zero;

        // Win zone (left, green)
        var winRT = MakeRect(trackRT, "WinZone", new Color(0.2f, 0.85f, 0.3f, 0.5f));
        winRT.anchorMin = Vector2.zero;
        winRT.anchorMax = new Vector2(0.15f, 1f);
        winRT.offsetMin = winRT.offsetMax = Vector2.zero;

        // Lose zone (right, red)
        var loseRT = MakeRect(trackRT, "LoseZone", new Color(0.9f, 0.2f, 0.2f, 0.5f));
        loseRT.anchorMin = new Vector2(0.85f, 0f);
        loseRT.anchorMax = Vector2.one;
        loseRT.offsetMin = loseRT.offsetMax = Vector2.zero;

        // Indicator (fish)
        var indRT = MakeRect(trackRT, "Indicator", Color.white);
        indRT.anchorMin = new Vector2(0.5f, 0.1f);
        indRT.anchorMax = new Vector2(0.5f, 0.9f);
        indRT.pivot = new Vector2(0.5f, 0.5f);
        indRT.sizeDelta = new Vector2(8f, 0f);

        var indLabel = MakeLabel(indRT, "🐟", 8f, Color.white);
        var indLabelRT = indLabel.GetComponent<RectTransform>();
        indLabelRT.anchorMin = Vector2.zero;
        indLabelRT.anchorMax = Vector2.one;
        indLabelRT.offsetMin = indLabelRT.offsetMax = Vector2.zero;

        var statusLabel = MakeLabel(content, "", 9f, Color.white);
        var statusRT = statusLabel.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0.05f, 0.3f);
        statusRT.anchorMax = new Vector2(0.95f, 0.44f);
        statusRT.offsetMin = statusRT.offsetMax = Vector2.zero;

        float pos = 0.5f;

        while (pos > 0.15f && pos < 0.85f)
        {
            // Fish surges periodically
            surgeTimer += Time.deltaTime;
            if (surgeTimer >= surgeInterval)
            {
                surgeTimer = 0f;
                surgeDuration = Random.Range(0.4f, 0.9f);
                surgeInterval = Mathf.Max(1.5f, surgeInterval * Random.Range(0.8f, 1.1f));
            }
            surgeDuration -= Time.deltaTime;
            float currentFishPull = surgeDuration > 0f ? fishPull * 2.2f : fishPull;

            pos += currentFishPull * Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space)) pos -= playerPull;
            pos = Mathf.Clamp01(pos);

            indRT.anchorMin = new Vector2(pos, 0.1f);
            indRT.anchorMax = new Vector2(pos, 0.9f);

            statusLabel.text = surgeDuration > 0f ? "It's surging! SPAM FASTER!" : "Keep reeling!";

            yield return null;
        }

        Destroy(minigameContent); minigameContent = null;
        EndMinigame(pos <= 0.15f);
    }

    // ── Debug / test entry point ───────────────────────────────────────────────
    // Called by DebugMenuManager to launch a specific minigame with fake fish.
    public void ForceMinigame(int typeIndex, List<FishData> fish)
    {
        currentFish = fish;
        int totalDifficulty = 0;
        foreach (var f in fish) totalDifficulty += f.difficulty;
        bubblesNeeded   = baseBubblesNeeded + totalDifficulty;
        bubbleLifetime  = Mathf.Max(minBubbleLifetime, baseBubbleLifetime - totalDifficulty * lifetimeReductionPerDifficulty);
        if (PerkManager.Instance != null)
        {
            bubblesNeeded += PerkManager.Instance.ExtraBubblesPerFish * fish.Count;
            bubblesNeeded  = Mathf.Max(bubblesNeeded, PerkManager.Instance.MinBubbles);
            currentAllowedMisses = allowedMisses + PerkManager.Instance.AllowedMissesBonus;
        }
        else currentAllowedMisses = allowedMisses;

        popped = 0; missed = 0;
        panel.gameObject.SetActive(true);
        var bgImg = panel.GetComponent<Image>();
        if (bgImg != null) bgImg.enabled = true;

        var type = (MinigameType)(typeIndex % System.Enum.GetValues(typeof(MinigameType)).Length);
        switch (type)
        {
            case MinigameType.BubblePop:  StartCoroutine(BubbleLoop()); break;
            case MinigameType.TimingBar:  StartCoroutine(TimingBarGame(totalDifficulty)); break;
            case MinigameType.ButtonMash: StartCoroutine(ButtonMashGame(totalDifficulty)); break;
            case MinigameType.HoldZone:   StartCoroutine(HoldZoneGame(totalDifficulty)); break;
            case MinigameType.TugOfWar:   StartCoroutine(TugOfWarGame(totalDifficulty)); break;
            case MinigameType.RingDodge:  StartCoroutine(RingDodgeGame(totalDifficulty)); break;
        }
    }

    // ── RING DODGE (modular) ────────────────────────────────────────────────────
    // To remove: delete `RingDodge` from MinigameType enum, delete the case in StartMinigame,
    // and delete everything from here to the matching END RING DODGE comment.

    private class ActiveRing
    {
        public List<(RectTransform rt, float angleDeg)> segs = new List<(RectTransform, float)>();
        public float gapCenter;      // degrees – centre of safe gap
        public float gapHalf;        // half-width of gap in degrees
        public float radius;         // current radius (shrinks each frame)
        public float startRadius;
        public float shrinkDuration;
        public float elapsed;
        public bool  hitChecked;
        public bool  alive = true;
    }

    IEnumerator RingDodgeGame(int difficulty)
    {
        var content = GetFreshContent();
        yield return null; // let layout settle so panel.rect is valid

        float pw = panel.rect.width;
        float ph = panel.rect.height;

        // ── Ring container (centred in content) ──────────────────────
        var ringContGO = new GameObject("RingContainer");
        ringContGO.transform.SetParent(content, false);
        var ringContRT = ringContGO.AddComponent<RectTransform>();
        ringContRT.anchorMin = ringContRT.anchorMax = new Vector2(0.5f, 0.5f);
        ringContRT.pivot     = new Vector2(0.5f, 0.5f);
        ringContRT.sizeDelta = Vector2.zero;
        ringContRT.anchoredPosition = Vector2.zero;

        // ── Difficulty params ─────────────────────────────────────────
        float gapMult      = PerkManager.Instance != null ? PerkManager.Instance.RingDodgeGapMultiplier      : 1f;
        float shrinkDMult  = PerkManager.Instance != null ? PerkManager.Instance.RingDodgeShrinkDurationMult  : 1f;
        float rotMult      = PerkManager.Instance != null ? PerkManager.Instance.RingDodgeRotSpeedMultiplier  : 1f;
        int   extraRings   = PerkManager.Instance != null ? PerkManager.Instance.RingDodgeExtraRings          : 0;

        float orbitRadius   = 28f;
        float startRadius   = Mathf.Min(pw, ph) * 0.52f;
        float shrinkDur     = Mathf.Max(1.6f, 4.8f - difficulty * 0.45f + Random.Range(-0.3f, 0.3f)) * shrinkDMult;
        float spawnInterval = Mathf.Max(1.0f, 3.2f - difficulty * 0.28f + Random.Range(-0.2f, 0.2f));
        float gapDeg        = Mathf.Max(26f,  70f  - difficulty * 6f   + Random.Range(-5f,  5f)) * gapMult;
        int   ringCount     = 4 + difficulty / 2 + extraRings;
        float rotSpeed      = (80f + difficulty * 8f) * rotMult; // degrees / second

        // ── Orbit guide dots ─────────────────────────────────────────
        for (int i = 0; i < 36; i++)
        {
            float a = i * 10f * Mathf.Deg2Rad;
            var dotGO = new GameObject("OD");
            dotGO.transform.SetParent(ringContRT, false);
            dotGO.AddComponent<Image>().color = new Color(0.4f, 0.65f, 1f, 0.25f);
            var dotRT = dotGO.GetComponent<RectTransform>();
            dotRT.anchorMin = dotRT.anchorMax = new Vector2(0.5f, 0.5f);
            dotRT.pivot      = new Vector2(0.5f, 0.5f);
            dotRT.sizeDelta  = new Vector2(2f, 2f);
            dotRT.anchoredPosition = new Vector2(Mathf.Cos(a) * orbitRadius, Mathf.Sin(a) * orbitRadius);
        }

        // ── Centre marker ─────────────────────────────────────────────
        var ctrGO = new GameObject("Centre");
        ctrGO.transform.SetParent(ringContRT, false);
        ctrGO.AddComponent<Image>().color = new Color(1f, 0.35f, 0.35f, 0.85f);
        var ctrRT = ctrGO.GetComponent<RectTransform>();
        ctrRT.anchorMin = ctrRT.anchorMax = new Vector2(0.5f, 0.5f);
        ctrRT.pivot      = new Vector2(0.5f, 0.5f);
        ctrRT.sizeDelta  = new Vector2(5f, 5f);
        ctrRT.anchoredPosition = Vector2.zero;

        // ── Player circle ─────────────────────────────────────────────
        var playerGO = new GameObject("Player");
        playerGO.transform.SetParent(ringContRT, false);
        playerGO.AddComponent<Image>().color = new Color(0.3f, 0.9f, 1f);
        var playerRT = playerGO.GetComponent<RectTransform>();
        playerRT.anchorMin = playerRT.anchorMax = new Vector2(0.5f, 0.5f);
        playerRT.pivot      = new Vector2(0.5f, 0.5f);
        playerRT.sizeDelta  = new Vector2(8f, 8f);

        // ── Labels ───────────────────────────────────────────────────
        var titleLabel = MakeLabel(content, "Dodge the rings!  A / D to rotate", 8f, Color.white);
        var titleRT = titleLabel.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.88f);
        titleRT.anchorMax = new Vector2(0.95f, 0.99f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        var statusLabel = MakeLabel(content, "", 8f, Color.white);
        var statusRT = statusLabel.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0.05f, 0.13f);
        statusRT.anchorMax = new Vector2(0.95f, 0.23f);
        statusRT.offsetMin = statusRT.offsetMax = Vector2.zero;

        // ── State ────────────────────────────────────────────────────
        float playerAngle  = 90f;
        var   rings        = new List<ActiveRing>();
        float spawnTimer   = spawnInterval; // fire first ring immediately
        int   ringsSpawned = 0;
        int   ringsSurvived = 0;
        bool  failed       = false;

        while (!failed && (ringsSurvived < ringCount || rings.Count > 0))
        {
            float dt = Time.deltaTime;

            // Player rotation: D = clockwise (angle decreases), A = counter-clockwise
            float h = Input.GetAxisRaw("Horizontal");
            playerAngle -= h * rotSpeed * dt;
            float pRad = playerAngle * Mathf.Deg2Rad;
            playerRT.anchoredPosition = new Vector2(Mathf.Cos(pRad) * orbitRadius, Mathf.Sin(pRad) * orbitRadius);

            // Spawn rings
            spawnTimer += dt;
            if (spawnTimer >= spawnInterval && ringsSpawned < ringCount)
            {
                spawnTimer = 0f;
                float gap = Random.Range(0f, 360f);
                var ring = new ActiveRing
                {
                    gapCenter     = gap,
                    gapHalf       = gapDeg * 0.5f,
                    radius        = startRadius,
                    startRadius   = startRadius,
                    shrinkDuration = shrinkDur,
                    elapsed       = 0f,
                    hitChecked    = false,
                    alive         = true,
                };
                ring.segs = SpawnRingSegments(ringContRT, gap, gapDeg, startRadius);
                rings.Add(ring);
                ringsSpawned++;
            }

            // Update rings
            for (int i = rings.Count - 1; i >= 0; i--)
            {
                var ring = rings[i];
                if (!ring.alive) { rings.RemoveAt(i); continue; }

                float prevRadius = ring.radius;
                ring.elapsed += dt;
                ring.radius = Mathf.Lerp(ring.startRadius, 0f, ring.elapsed / ring.shrinkDuration);

                // Move segments inward
                foreach (var (rt, angleDeg) in ring.segs)
                {
                    if (rt == null) continue;
                    float aRad = angleDeg * Mathf.Deg2Rad;
                    rt.anchoredPosition = new Vector2(Mathf.Cos(aRad) * ring.radius, Mathf.Sin(aRad) * ring.radius);
                }

                // Hit-check when ring crosses player's orbit radius
                if (!ring.hitChecked && prevRadius > orbitRadius && ring.radius <= orbitRadius)
                {
                    ring.hitChecked = true;
                    float delta = Mathf.DeltaAngle(ring.gapCenter, playerAngle);
                    if (Mathf.Abs(delta) <= ring.gapHalf)
                        ringsSurvived++;
                    else
                        failed = true;
                }

                // Destroy ring once it reaches the centre
                if (ring.elapsed >= ring.shrinkDuration)
                {
                    foreach (var (rt, _) in ring.segs) if (rt != null) Destroy(rt.gameObject);
                    ring.alive = false;
                }
            }

            statusLabel.text = "Rings cleared: " + ringsSurvived + " / " + ringCount;
            yield return null;
        }

        // Clean up any leftover segments before destroying content
        foreach (var ring in rings)
            foreach (var (rt, _) in ring.segs)
                if (rt != null) Destroy(rt.gameObject);

        Destroy(minigameContent); minigameContent = null;
        EndMinigame(!failed);
    }

    // Spawns arc segments for one ring, leaving a gap around gapCenter (degrees).
    static List<(RectTransform, float)> SpawnRingSegments(
        RectTransform parent, float gapCenter, float gapDeg, float radius)
    {
        var list    = new List<(RectTransform, float)>();
        float step  = 8f;
        float half  = gapDeg * 0.5f;

        for (float a = 0f; a < 360f; a += step)
        {
            // Skip segments inside the gap (with a half-step margin so edges are clean)
            if (Mathf.Abs(Mathf.DeltaAngle(gapCenter, a)) <= half + step * 0.5f) continue;

            var go = new GameObject("Seg");
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = new Color(0.95f, 0.28f, 0.18f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot      = new Vector2(0.5f, 0.5f);
            rt.sizeDelta  = new Vector2(10f, 5f);

            float aRad = a * Mathf.Deg2Rad;
            rt.anchoredPosition  = new Vector2(Mathf.Cos(aRad) * radius, Mathf.Sin(aRad) * radius);
            rt.localEulerAngles  = new Vector3(0f, 0f, a + 90f); // tangent to ring

            list.Add((rt, a));
        }
        return list;
    }

    // ── END RING DODGE ──────────────────────────────────────────────────────────
}
