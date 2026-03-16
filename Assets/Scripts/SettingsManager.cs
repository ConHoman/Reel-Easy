using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        // Deselect any focused UI element so Space doesn't re-trigger buttons during minigames
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
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
        panelRT.anchorMin = new Vector2(0.18f, 0.08f);
        panelRT.anchorMax = new Vector2(0.82f, 0.92f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        // Title bar
        var titleBar = MakeRect(settingsPanel.transform, "TitleBar", new Color(0.08f, 0.11f, 0.24f, 1f));
        titleBar.anchorMin = new Vector2(0f, 0.88f);
        titleBar.anchorMax = Vector2.one;
        titleBar.offsetMin = titleBar.offsetMax = Vector2.zero;
        MakeLabel(titleBar, "Settings", 10f, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

        // ── Hints toggle ──────────────────────────────────────
        var hintsBG = MakeRect(settingsPanel.transform, "HintsBG", new Color(0.08f, 0.1f, 0.2f, 1f));
        hintsBG.anchorMin = new Vector2(0.05f, 0.76f);
        hintsBG.anchorMax = new Vector2(0.95f, 0.85f);
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
        hintsRT.anchorMin = new Vector2(0.05f, 0.76f);
        hintsRT.anchorMax = new Vector2(0.95f, 0.85f);
        hintsRT.offsetMin = hintsRT.offsetMax = Vector2.zero;
        RefreshLabel();
        hintsBtn.onClick.AddListener(ToggleHints);

        // ── Controls reference ────────────────────────────────
        MakeLabel(settingsPanel.transform, "CONTROLS", 6f,
            new Color(0.5f, 0.5f, 0.65f), TextAlignmentOptions.Left,
            new Vector2(0.05f, 0.66f), new Vector2(0.95f, 0.74f));

        // Two-column layout: keys left, actions right
        string[] keys = { "WASD / Arrows", "F", "Space", "E", "Tab", "Q", "Escape" };
        string[] actions = { "Move / Steer bobber", "Cast line", "Minigames", "Fish inventory", "Fish compendium", "Active perks", "Settings" };
        Color rowColor = new Color(0.75f, 0.75f, 0.85f);

        var keysGO = new GameObject("CtrlKeys");
        keysGO.transform.SetParent(settingsPanel.transform, false);
        var keysTMP = keysGO.AddComponent<TextMeshProUGUI>();
        keysTMP.text = string.Join("\n", keys);
        keysTMP.fontSize = 5.5f;
        keysTMP.color = rowColor;
        keysTMP.alignment = TextAlignmentOptions.TopLeft;
        keysTMP.enableWordWrapping = false;
        keysTMP.lineSpacing = 8f;
        var keysRT = keysGO.GetComponent<RectTransform>();
        keysRT.anchorMin = new Vector2(0.07f, 0.17f);
        keysRT.anchorMax = new Vector2(0.42f, 0.64f);
        keysRT.offsetMin = keysRT.offsetMax = Vector2.zero;

        var actionsGO = new GameObject("CtrlActions");
        actionsGO.transform.SetParent(settingsPanel.transform, false);
        var actionsTMP = actionsGO.AddComponent<TextMeshProUGUI>();
        actionsTMP.text = string.Join("\n", actions);
        actionsTMP.fontSize = 5.5f;
        actionsTMP.color = rowColor;
        actionsTMP.alignment = TextAlignmentOptions.TopLeft;
        actionsTMP.enableWordWrapping = false;
        actionsTMP.lineSpacing = 8f;
        var actionsRT = actionsGO.GetComponent<RectTransform>();
        actionsRT.anchorMin = new Vector2(0.44f, 0.17f);
        actionsRT.anchorMax = new Vector2(0.97f, 0.64f);
        actionsRT.offsetMin = actionsRT.offsetMax = Vector2.zero;

        // ── Restart Run button ────────────────────────────────
        MakeButton(settingsPanel.transform, "Restart Run",
            new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.13f),
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
