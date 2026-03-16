using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Press ` (backtick) to toggle.
// LAUNCH tab  — pick a minigame type, set params, launch it.
// PERKS  tab  — click any perk to add a stack; click again to remove one stack.
public class DebugMenuManager : MonoBehaviour
{
    public static DebugMenuManager Instance;

    private GameObject  debugPanel;
    private bool        panelOpen = false;

    // ── Launch state ─────────────────────────────────────────────────────────
    private int selectedMinigame = 0;
    private int launchDifficulty = 5;
    private int launchRarity     = 2;
    private int launchFishCount  = 1;

    // ── UI references ────────────────────────────────────────────────────────
    private Button[]   mgButtons;
    private Button[]   perkButtons;
    private TMP_Text[] perkLabels;   // label inside each perk button (shows name + count)
    private TMP_Text   detailText;   // bottom panel — shows selected perk info
    private Button     tabLaunchBtn, tabPerksBtn;
    private GameObject launchContent, perksContent;
    private int        activeTab = 0;

    private static readonly string[] MgLabels =
        { "Bubble Pop", "Timing Bar", "Button Mash", "Hold Zone", "Tug of War", "Ring Dodge" };

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
            new GameObject("DebugMenuManager").AddComponent<DebugMenuManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
            SetOpen(!panelOpen);
    }

    void SetOpen(bool open)
    {
        panelOpen = open;
        debugPanel.SetActive(open);
        if (open) RefreshPerkButtons();
    }

    void SwitchTab(int tab)
    {
        activeTab = tab;
        launchContent.SetActive(tab == 0);
        perksContent.SetActive(tab == 1);
        tabLaunchBtn.GetComponent<Image>().color = tab == 0
            ? new Color(0.15f, 0.40f, 0.15f) : new Color(0.10f, 0.12f, 0.22f);
        tabPerksBtn.GetComponent<Image>().color = tab == 1
            ? new Color(0.15f, 0.40f, 0.15f) : new Color(0.10f, 0.12f, 0.22f);
    }

    // ── Top-level panel ───────────────────────────────────────────────────────

    void BuildUI()
    {
        Canvas canvas = UICanvas.Get();

        // Outer panel — nearly full screen
        debugPanel = new GameObject("DebugPanel");
        debugPanel.transform.SetParent(canvas.transform, false);
        debugPanel.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.13f, 0.97f);
        var panelRT = debugPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.03f, 0.04f);
        panelRT.anchorMax = new Vector2(0.97f, 0.96f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        Transform p = debugPanel.transform;

        // Title bar  (top 8% of panel)
        var titleRT = DBRect(p, "TitleBar", new Color(0.50f, 0.13f, 0.04f));
        titleRT.anchorMin = new Vector2(0f,   0.92f);
        titleRT.anchorMax = Vector2.one;
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        var titleLbl = DBLabel(titleRT, "DEBUG MENU", 7f, Color.white);
        var tlRT = titleLbl.GetComponent<RectTransform>();
        tlRT.anchorMin = new Vector2(0.02f, 0f); tlRT.anchorMax = new Vector2(0.78f, 1f);
        tlRT.offsetMin = tlRT.offsetMax = Vector2.zero;
        titleLbl.alignment = TextAlignmentOptions.Left;

        // Close button sits in the title bar area but child of panel so it's clickable on top
        var closeBtn = DBButton(p, "X  [`]",
            new Vector2(0.80f, 0.925f), new Vector2(0.99f, 0.990f),
            new Color(0.55f, 0.08f, 0.08f), 6f);
        closeBtn.onClick.AddListener(() => SetOpen(false));

        // Tab buttons  (y = 0.84 – 0.91)
        tabLaunchBtn = DBButton(p, "LAUNCH",
            new Vector2(0.01f, 0.84f), new Vector2(0.49f, 0.91f),
            new Color(0.15f, 0.40f, 0.15f), 6.5f);
        tabLaunchBtn.onClick.AddListener(() => SwitchTab(0));

        tabPerksBtn = DBButton(p, "PERKS",
            new Vector2(0.51f, 0.84f), new Vector2(0.99f, 0.91f),
            new Color(0.10f, 0.12f, 0.22f), 6.5f);
        tabPerksBtn.onClick.AddListener(() => SwitchTab(1));

        // Content areas  (y = 0.01 – 0.83, leaving gap below tab buttons)
        launchContent = MakeContentGO(p, "LaunchContent");
        BuildLaunchTab(launchContent.transform);

        perksContent = MakeContentGO(p, "PerksContent");
        BuildPerksTab(perksContent.transform);

        debugPanel.SetActive(false);
        SwitchTab(0);
        SelectMinigame(0);
    }

    static GameObject MakeContentGO(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.01f);
        rt.anchorMax = new Vector2(1f, 0.83f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    // ── Launch tab ────────────────────────────────────────────────────────────
    // All anchor values are within the content GO (0–1 maps to content rect).
    // Laid out top → bottom so nothing overlaps.

    void BuildLaunchTab(Transform t)
    {
        // "SELECT MINIGAME" header  ──── y 0.90 – 0.99
        DBSectionHeader(t, "SELECT MINIGAME", 0.90f, 0.99f);

        // Minigame buttons — two rows of three  ──── row0: 0.76–0.88  row1: 0.62–0.74
        mgButtons = new Button[MgLabels.Length];
        const float bw = 1f / 3f;
        for (int i = 0; i < MgLabels.Length; i++)
        {
            int idx = i;
            int row = i / 3, col = i % 3;
            float yBot = 0.76f - row * 0.145f;   // row 0 → 0.76, row 1 → 0.615
            float yTop = yBot + 0.120f;
            var btn = DBButton(t, MgLabels[i],
                new Vector2(col * bw + 0.012f, yBot),
                new Vector2((col + 1) * bw - 0.012f, yTop),
                new Color(0.14f, 0.14f, 0.30f), 6f);
            btn.onClick.AddListener(() => SelectMinigame(idx));
            mgButtons[i] = btn;
        }

        // Divider  ──── y 0.585 – 0.600
        var div = DBRect(t, "Div", new Color(0.30f, 0.30f, 0.50f, 0.40f));
        div.anchorMin = new Vector2(0.02f, 0.588f);
        div.anchorMax = new Vector2(0.98f, 0.594f);
        div.offsetMin = div.offsetMax = Vector2.zero;

        // Step controls  ──── stacked downward from y 0.555
        const float rh  = 0.095f;   // row height
        const float gap = 0.010f;   // gap between rows
        float top = 0.555f;         // top of first step control

        DBSectionHeader(t, "PARAMS", top + 0.005f, top + 0.040f);
        top -= 0.048f; // move below the "PARAMS" label

        MakeStepControl(t, "Difficulty:", launchDifficulty, 1, 20, top - rh,         v => launchDifficulty = v);
        MakeStepControl(t, "Fish Rarity:", launchRarity,    1,  3, top - rh * 2 - gap, v => launchRarity = v);
        MakeStepControl(t, "Fish Count:",  launchFishCount, 1,  8, top - rh * 3 - gap * 2, v => launchFishCount = v);

        // LAUNCH button (right) + GIVE ALL FISH button (left)  ──── y 0.03 – 0.16
        var giveAllBtn = DBButton(t, "GIVE ALL FISH",
            new Vector2(0.02f, 0.03f), new Vector2(0.44f, 0.16f),
            new Color(0.10f, 0.28f, 0.42f), 6f);
        giveAllBtn.onClick.AddListener(GiveAllFish);

        var launchBtn = DBButton(t, "LAUNCH  ▶",
            new Vector2(0.46f, 0.03f), new Vector2(0.98f, 0.16f),
            new Color(0.08f, 0.50f, 0.08f), 8.5f);
        launchBtn.onClick.AddListener(LaunchMinigame);
    }

    // ── Perks tab ─────────────────────────────────────────────────────────────

    void BuildPerksTab(Transform t)
    {
        int total = PerkManager.AllPerks.Length;
        const int cols = 3;
        int rows = (total + cols - 1) / cols;

        // Grid occupies y = 0.22 – 0.99  (top 77%)
        const float gridTop = 0.99f, gridBot = 0.22f;
        float gridH = gridTop - gridBot;
        float rh    = gridH / rows - 0.005f;   // row height with small gap
        float bw    = 1f / cols;

        perkButtons = new Button[total];
        perkLabels  = new TMP_Text[total];

        for (int i = 0; i < total; i++)
        {
            int idx = i;
            int row = i / cols, col = i % cols;
            float yTop = gridTop - row * (rh + 0.005f);
            float yBot = yTop - rh;

            var btn = DBButton(t, "",   // label text set below
                new Vector2(col * bw + 0.010f, yBot),
                new Vector2((col + 1) * bw - 0.010f, yTop),
                new Color(0.12f, 0.12f, 0.24f), 5.5f);

            // Replace the label TMP so we can update it
            var lbl = btn.GetComponentInChildren<TMP_Text>();
            perkButtons[i] = btn;
            perkLabels[i]  = lbl;

            btn.onClick.AddListener(() => OnPerkClick(idx));
        }

        // Detail panel background  ──── y 0.08 – 0.21
        var detailBG = DBRect(t, "DetailBG", new Color(0.07f, 0.09f, 0.17f, 1f));
        detailBG.anchorMin = new Vector2(0f,    0.08f);
        detailBG.anchorMax = new Vector2(0.70f, 0.21f);
        detailBG.offsetMin = detailBG.offsetMax = Vector2.zero;

        // Detail text inside the background
        detailText = DBLabel(detailBG, "<color=#888>Click a perk to see details.</color>", 4.5f, Color.white);
        detailText.alignment    = TextAlignmentOptions.Left;
        detailText.enableWordWrapping = true;
        var dtRT = detailText.GetComponent<RectTransform>();
        dtRT.anchorMin = new Vector2(0.02f, 0f); dtRT.anchorMax = new Vector2(0.98f, 1f);
        dtRT.offsetMin = dtRT.offsetMax = Vector2.zero;

        // Clear All button  ──── y 0.08 – 0.21, right side
        var clearBtn = DBButton(t, "Clear All",
            new Vector2(0.72f, 0.08f), new Vector2(0.99f, 0.21f),
            new Color(0.40f, 0.08f, 0.08f), 6f);
        clearBtn.onClick.AddListener(ClearAllPerks);

        // "click = add  right-click = remove" hint
        var hint = DBLabel(t, "click = add stack    right-click = remove", 4f, new Color(0.5f, 0.5f, 0.7f));
        var hRT  = hint.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0f, 0.01f); hRT.anchorMax = new Vector2(0.70f, 0.07f);
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;
        hint.alignment = TextAlignmentOptions.Left;

        RefreshPerkButtons();
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    void SelectMinigame(int idx)
    {
        selectedMinigame = idx;
        for (int i = 0; i < mgButtons.Length; i++)
            mgButtons[i].GetComponent<Image>().color = i == idx
                ? new Color(0.15f, 0.45f, 0.15f) : new Color(0.14f, 0.14f, 0.30f);
    }

    void OnPerkClick(int idx)
    {
        if (PerkManager.Instance == null) return;

        var def  = PerkManager.AllPerks[idx];
        bool remove = Input.GetMouseButton(1); // right-click removes

        if (remove) PerkManager.Instance.RemovePerk(def.type);
        else        PerkManager.Instance.AddPerk(def.type);

        // Show details in the bottom panel
        int stacks = PerkManager.Instance.Count(def.type);
        string stackStr = stacks > 1 ? $"  ×{stacks}" : "";
        detailText.text =
            $"<b><color=#FFFFFF>{def.displayName}{stackStr}</color></b>  " +
            $"<color=#AAAAFF>[{def.category}]</color>\n" +
            $"<color=#66FF88>+ {def.upside}</color>" +
            (string.IsNullOrEmpty(def.downside) ? "" : $"\n<color=#FF7777>- {def.downside}</color>");

        RefreshPerkButtons();
    }

    void ClearAllPerks()
    {
        if (PerkManager.Instance != null) PerkManager.Instance.ResetForRun();
        if (detailText != null) detailText.text = "<color=#888>Click a perk to see details.</color>";
        RefreshPerkButtons();
    }

    void RefreshPerkButtons()
    {
        if (perkButtons == null || PerkManager.Instance == null) return;
        for (int i = 0; i < perkButtons.Length; i++)
        {
            var  def    = PerkManager.AllPerks[i];
            int  stacks = PerkManager.Instance.Count(def.type);
            bool on     = stacks > 0;

            perkButtons[i].GetComponent<Image>().color = on
                ? CategoryColorBright(def.category) : new Color(0.12f, 0.12f, 0.24f);

            string badge = stacks > 1 ? $" ×{stacks}" : "";
            perkLabels[i].text = def.displayName + badge;
        }
    }

    static Color CategoryColorBright(PerkCategory cat)
    {
        switch (cat)
        {
            case PerkCategory.Safety:   return new Color(0.15f, 0.25f, 0.60f);
            case PerkCategory.Fishing:  return new Color(0.10f, 0.42f, 0.38f);
            case PerkCategory.Scoring:  return new Color(0.48f, 0.36f, 0.05f);
            case PerkCategory.Minigame: return new Color(0.34f, 0.10f, 0.48f);
            default:                    return new Color(0.20f, 0.20f, 0.40f);
        }
    }

    void GiveAllFish()
    {
        if (FishJournal.Instance == null) return;
        foreach (var fish in FishDatabase.CreateAll())
            FishJournal.Instance.Discover(fish.fishName);
    }

    void LaunchMinigame()
    {
        var mm = FindObjectOfType<MinigameManager>();
        if (mm == null) { Debug.LogWarning("[Debug] No MinigameManager in scene."); return; }

        var fish = new List<FishData>();
        for (int i = 0; i < launchFishCount; i++)
        {
            var fd = ScriptableObject.CreateInstance<FishData>();
            fd.fishName   = "Debug Fish";
            fd.difficulty = launchDifficulty;
            fd.rarity     = launchRarity;
            fd.scoreValue = 100 * launchRarity * launchDifficulty;
            fish.Add(fd);
        }

        SetOpen(false);
        mm.ForceMinigame(selectedMinigame, fish);
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    static RectTransform DBRect(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    static TMP_Text DBLabel(Transform parent, string text, float size, Color color)
    {
        var go = new GameObject("Lbl");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return tmp;
    }

    static void DBSectionHeader(Transform parent, string text, float yBot, float yTop)
    {
        var go = new GameObject("SHdr");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 5.5f;
        tmp.color     = new Color(0.55f, 0.55f, 0.82f);
        tmp.alignment = TextAlignmentOptions.Left;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.02f, yBot);
        rt.anchorMax = new Vector2(0.98f, yTop);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Button DBButton(Transform parent, string label,
        Vector2 aMin, Vector2 aMax, Color bg, float fontSize = 5.5f)
    {
        var go = new GameObject("Btn");
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bg;
        var btn = go.AddComponent<Button>();
        var cs  = btn.colors;
        cs.highlightedColor = new Color(Mathf.Min(1f, bg.r + 0.12f), Mathf.Min(1f, bg.g + 0.12f), Mathf.Min(1f, bg.b + 0.12f));
        cs.pressedColor     = new Color(Mathf.Max(0f, bg.r - 0.12f), Mathf.Max(0f, bg.g - 0.12f), Mathf.Max(0f, bg.b - 0.12f));
        btn.colors = cs;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        DBLabel(go.transform, label, fontSize, Color.white);
        return btn;
    }

    // [Label text]   [−]  [value]  [+]
    static void MakeStepControl(Transform parent, string label, int defaultVal,
        int min, int max, float yBot, System.Action<int> onChange)
    {
        float yTop = yBot + 0.090f;

        // label (left 38%)
        var lblGO = new GameObject("SL");
        lblGO.transform.SetParent(parent, false);
        var lblT = lblGO.AddComponent<TextMeshProUGUI>();
        lblT.text = label; lblT.fontSize = 6f;
        lblT.color = new Color(0.82f, 0.82f, 0.82f);
        lblT.alignment = TextAlignmentOptions.Right;
        var lRT = lblGO.GetComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0.02f, yBot); lRT.anchorMax = new Vector2(0.40f, yTop);
        lRT.offsetMin = lRT.offsetMax = Vector2.zero;

        // − button
        var minus = DBButton(parent, "−",
            new Vector2(0.42f, yBot), new Vector2(0.54f, yTop),
            new Color(0.28f, 0.10f, 0.10f), 8f);

        // value display
        var valGO = new GameObject("SV");
        valGO.transform.SetParent(parent, false);
        var valT = valGO.AddComponent<TextMeshProUGUI>();
        valT.text = defaultVal.ToString(); valT.fontSize = 7f;
        valT.color = new Color(1f, 0.90f, 0.30f);
        valT.alignment = TextAlignmentOptions.Center;
        var vRT = valGO.GetComponent<RectTransform>();
        vRT.anchorMin = new Vector2(0.54f, yBot); vRT.anchorMax = new Vector2(0.67f, yTop);
        vRT.offsetMin = vRT.offsetMax = Vector2.zero;

        // + button
        var plus = DBButton(parent, "+",
            new Vector2(0.67f, yBot), new Vector2(0.79f, yTop),
            new Color(0.10f, 0.28f, 0.10f), 8f);

        int[] cur = { defaultVal };
        minus.onClick.AddListener(() => { cur[0] = Mathf.Max(min, cur[0] - 1); valT.text = cur[0].ToString(); onChange(cur[0]); });
        plus.onClick.AddListener(()  => { cur[0] = Mathf.Min(max, cur[0] + 1); valT.text = cur[0].ToString(); onChange(cur[0]); });
    }
}
