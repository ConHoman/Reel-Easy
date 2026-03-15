using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    private const string HintsKey = "Settings_ShowHints";
    public static bool ShowHints { get; private set; } = true;

    private GameObject settingsPanel;
    private TMP_Text hintsToggleLabel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ShowHints = PlayerPrefs.GetInt(HintsKey, 1) == 1;
        BuildUI();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
            new GameObject("SettingsManager").AddComponent<SettingsManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && settingsPanel != null)
            SetOpen(!settingsPanel.activeSelf);
    }

    void SetOpen(bool open)
    {
        settingsPanel.SetActive(open);
        // Pause while settings is open, but don't fight the perk picker (timeScale 0)
        if (open && Time.timeScale != 0f) Time.timeScale = 0f;
        else if (!open)                   Time.timeScale = 1f;
    }

    void BuildUI()
    {
        Canvas canvas = UICanvas.Get();
        if (canvas == null) return;

        // ── Gear button (top-right) ───────────────────────────
        var gearGO = new GameObject("SettingsGearButton");
        gearGO.transform.SetParent(canvas.transform, false);
        gearGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
        var gearBtn = gearGO.AddComponent<Button>();
        gearBtn.transition = Selectable.Transition.ColorTint;
        var gearRT = gearGO.GetComponent<RectTransform>();
        gearRT.anchorMin = gearRT.anchorMax = gearRT.pivot = new Vector2(1f, 1f);
        gearRT.anchoredPosition = new Vector2(-4f, -4f);
        gearRT.sizeDelta = new Vector2(12f, 12f);

        var gearLabelGO = new GameObject("Label");
        gearLabelGO.transform.SetParent(gearGO.transform, false);
        var gearLabel = gearLabelGO.AddComponent<TextMeshProUGUI>();
        gearLabel.text = "=";
        gearLabel.fontSize = 7f;
        gearLabel.alignment = TextAlignmentOptions.Center;
        gearLabel.color = Color.white;
        var gearLabelRT = gearLabelGO.GetComponent<RectTransform>();
        gearLabelRT.anchorMin = Vector2.zero;
        gearLabelRT.anchorMax = Vector2.one;
        gearLabelRT.offsetMin = gearLabelRT.offsetMax = Vector2.zero;
        gearBtn.onClick.AddListener(() => SetOpen(!settingsPanel.activeSelf));

        // ── Settings panel (centered) ─────────────────────────
        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);
        settingsPanel.AddComponent<Image>().color = new Color(0.05f, 0.07f, 0.14f, 0.97f);
        var panelRT = settingsPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.2f, 0.15f);
        panelRT.anchorMax = new Vector2(0.8f, 0.85f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        // Title bar
        var titleBar = MakeRect(settingsPanel.transform, "TitleBar", new Color(0.08f, 0.11f, 0.24f, 1f));
        titleBar.anchorMin = new Vector2(0f, 0.88f);
        titleBar.anchorMax = Vector2.one;
        titleBar.offsetMin = titleBar.offsetMax = Vector2.zero;
        MakeLabel(titleBar, "Settings", 10f, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

        // ── Hints toggle ──────────────────────────────────────
        var hintsBG = MakeRect(settingsPanel.transform, "HintsBG", new Color(0.08f, 0.1f, 0.2f, 1f));
        hintsBG.anchorMin = new Vector2(0.05f, 0.72f);
        hintsBG.anchorMax = new Vector2(0.95f, 0.84f);
        hintsBG.offsetMin = hintsBG.offsetMax = Vector2.zero;

        var hintsGO = new GameObject("HintsToggle");
        hintsGO.transform.SetParent(settingsPanel.transform, false);
        var hintsBtn = hintsGO.AddComponent<Button>();
        hintsBtn.transition = Selectable.Transition.None;
        hintsToggleLabel = hintsGO.AddComponent<TextMeshProUGUI>();
        hintsToggleLabel.fontSize = 7f;
        hintsToggleLabel.alignment = TextAlignmentOptions.Center;
        hintsToggleLabel.color = Color.white;
        var hintsRT = hintsGO.GetComponent<RectTransform>();
        hintsRT.anchorMin = new Vector2(0.05f, 0.72f);
        hintsRT.anchorMax = new Vector2(0.95f, 0.84f);
        hintsRT.offsetMin = hintsRT.offsetMax = Vector2.zero;
        RefreshLabel();
        hintsBtn.onClick.AddListener(ToggleHints);

        // ── Controls reference ────────────────────────────────
        var ctrlHeader = MakeLabel(settingsPanel.transform, "CONTROLS", 6f,
            new Color(0.5f, 0.5f, 0.65f), TextAlignmentOptions.Left,
            new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.68f));

        string[] controls = new[]
        {
            "WASD / Arrow Keys   Move / Steer bobber",
            "F                   Cast line",
            "Space               Minigames",
            "E                   Fish inventory",
            "Tab                 Fish compendium",
            "Escape              Settings",
        };

        float rowH = 0.07f;
        float y = 0.55f;
        foreach (string line in controls)
        {
            y -= rowH;
            MakeLabel(settingsPanel.transform, line, 6f, new Color(0.75f, 0.75f, 0.85f),
                TextAlignmentOptions.Left, new Vector2(0.07f, y), new Vector2(0.95f, y + rowH));
        }

        // ── Restart Run button ────────────────────────────────
        MakeButton(settingsPanel.transform, "Restart Run",
            new Vector2(0.1f, 0.03f), new Vector2(0.9f, 0.13f),
            new Color(0.55f, 0.15f, 0.15f, 1f), () =>
            {
                SetOpen(false);
                if (RunManager.Instance != null) RunManager.Instance.StartRun();
            });

        settingsPanel.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static RectTransform MakeRect(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    static TMP_Text MakeLabel(Transform parent, string text, float size, Color color,
        TextAlignmentOptions align, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(4f, 0f);
        rt.offsetMax = Vector2.zero;
        return tmp;
    }

    static void MakeButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax,
        Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bgColor;
        var btn = go.AddComponent<Button>();
        var cs = btn.colors;
        cs.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f);
        cs.pressedColor     = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
        btn.colors = cs;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        btn.onClick.AddListener(onClick);

        var txt = MakeLabel(go.transform, label, 8f, Color.white, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one);
        txt.GetComponent<RectTransform>().offsetMin = txt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
    }

    void ToggleHints() => SetShowHints(!ShowHints);

    public void SetShowHints(bool value)
    {
        ShowHints = value;
        PlayerPrefs.SetInt(HintsKey, value ? 1 : 0);
        PlayerPrefs.Save();
        RefreshLabel();
    }

    void RefreshLabel()
    {
        if (hintsToggleLabel != null)
            hintsToggleLabel.text = (ShowHints ? "[x]" : "[ ]") + "  Fishing hints";
    }
}
