using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Creates its own settings button + panel at runtime. No setup required.
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

    void BuildUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;


        // ── Settings panel (hidden by default) ───────────────
        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);
        settingsPanel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        var panelRT = settingsPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = panelRT.pivot = new Vector2(1f, 1f);
        panelRT.anchoredPosition = new Vector2(-4f, -22f);
        panelRT.sizeDelta = new Vector2(148f, 36f);

        // Hints toggle button inside panel
        var toggleGO = new GameObject("HintsToggle");
        toggleGO.transform.SetParent(settingsPanel.transform, false);
        var toggleBtn = toggleGO.AddComponent<Button>();
        toggleBtn.transition = Selectable.Transition.None;
        hintsToggleLabel = toggleGO.AddComponent<TextMeshProUGUI>();
        hintsToggleLabel.fontSize = 9f;
        hintsToggleLabel.alignment = TextAlignmentOptions.Left;
        hintsToggleLabel.color = Color.white;
        var toggleRT = toggleGO.GetComponent<RectTransform>();
        toggleRT.anchorMin = Vector2.zero;
        toggleRT.anchorMax = Vector2.one;
        toggleRT.offsetMin = new Vector2(8f, 4f);
        toggleRT.offsetMax = new Vector2(-8f, -4f);
        RefreshLabel();
        toggleBtn.onClick.AddListener(ToggleHints);

        settingsPanel.SetActive(false);

        // ── Gear button ───────────────────────────────────────
        var gearGO = new GameObject("SettingsGearButton");
        gearGO.transform.SetParent(canvas.transform, false);
        gearGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
        var gearBtn = gearGO.AddComponent<Button>();
        gearBtn.transition = Selectable.Transition.ColorTint;
        var gearRT = gearGO.GetComponent<RectTransform>();
        gearRT.anchorMin = gearRT.anchorMax = gearRT.pivot = new Vector2(1f, 1f);
        gearRT.anchoredPosition = new Vector2(-4f, -4f);
        gearRT.sizeDelta = new Vector2(18f, 18f);

        var gearLabelGO = new GameObject("Label");
        gearLabelGO.transform.SetParent(gearGO.transform, false);
        var gearLabel = gearLabelGO.AddComponent<TextMeshProUGUI>();
        gearLabel.text = "=";   // simple settings icon substitute
        gearLabel.fontSize = 9f;
        gearLabel.alignment = TextAlignmentOptions.Center;
        gearLabel.color = Color.white;
        var gearLabelRT = gearLabelGO.GetComponent<RectTransform>();
        gearLabelRT.anchorMin = Vector2.zero;
        gearLabelRT.anchorMax = Vector2.one;
        gearLabelRT.offsetMin = gearLabelRT.offsetMax = Vector2.zero;

        gearBtn.onClick.AddListener(() => settingsPanel.SetActive(!settingsPanel.activeSelf));
    }

    void ToggleHints()
    {
        SetShowHints(!ShowHints);
    }

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
            hintsToggleLabel.text = (ShowHints ? "[x] " : "[ ] ") + "Fishing hints";
    }
}
