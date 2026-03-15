using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Press Tab to open/close. Shows all fish with stats; undiscovered appear as ???.
public class FishCompendium : MonoBehaviour
{
    public static FishCompendium Instance;

    private GameObject panel;
    private Transform listContainer;
    private TMP_Text titleLabel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
            new GameObject("FishCompendium").AddComponent<FishCompendium>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && panel != null)
        {
            bool opening = !panel.activeSelf;
            panel.SetActive(opening);
            if (opening) RefreshList();
        }
    }

    // ── UI Construction ────────────────────────────────────────────────────────

    void BuildUI()
    {
        Canvas canvas = UICanvas.Get();

        panel = new GameObject("FishCompendium");
        panel.transform.SetParent(canvas.transform, false);
        panel.AddComponent<Image>().color = new Color(0.04f, 0.07f, 0.14f, 0.97f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.08f);
        panelRT.anchorMax = new Vector2(0.9f, 0.92f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        // Title bar
        var titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(panel.transform, false);
        titleBar.AddComponent<Image>().color = new Color(0.07f, 0.1f, 0.22f, 1f);
        var titleBarRT = titleBar.GetComponent<RectTransform>();
        titleBarRT.anchorMin = new Vector2(0f, 0.88f);
        titleBarRT.anchorMax = Vector2.one;
        titleBarRT.offsetMin = titleBarRT.offsetMax = Vector2.zero;

        titleLabel = MakeLabel(titleBar.transform, "Fish Compendium", 9f, Color.white, TextAlignmentOptions.Center);
        var tlRT = titleLabel.GetComponent<RectTransform>();
        tlRT.anchorMin = Vector2.zero;
        tlRT.anchorMax = Vector2.one;
        tlRT.offsetMin = tlRT.offsetMax = Vector2.zero;

        // Column headers
        var headers = new GameObject("Headers");
        headers.transform.SetParent(panel.transform, false);
        headers.AddComponent<Image>().color = new Color(0.09f, 0.13f, 0.26f, 1f);
        var headersRT = headers.GetComponent<RectTransform>();
        headersRT.anchorMin = new Vector2(0f, 0.79f);
        headersRT.anchorMax = new Vector2(1f, 0.88f);
        headersRT.offsetMin = headersRT.offsetMax = Vector2.zero;

        MakeColumnLabel(headers.transform, "FISH",       0.03f, 0.42f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "RARITY",     0.42f, 0.62f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "DIFFICULTY", 0.62f, 0.82f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "SCORE",      0.82f, 1.00f, new Color(0.6f, 0.6f, 0.7f));

        // Scroll area
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(panel.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0.05f);
        scrollRT.anchorMax = new Vector2(1f, 0.79f);
        scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;

        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10f;

        var maskGO = new GameObject("Mask");
        maskGO.transform.SetParent(scrollGO.transform, false);
        var maskRT = maskGO.AddComponent<RectTransform>();
        maskRT.anchorMin = Vector2.zero;
        maskRT.anchorMax = Vector2.one;
        maskRT.offsetMin = maskRT.offsetMax = Vector2.zero;
        maskGO.AddComponent<Image>().color = Color.white;
        var mask = maskGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(maskGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = Vector2.one;
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;

        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 1f;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true;

        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = maskRT;
        scrollRect.content = contentRT;
        listContainer = contentGO.transform;

        // Hint
        var hint = MakeLabel(panel.transform, "Tab to close", 6f, new Color(0.4f, 0.4f, 0.5f), TextAlignmentOptions.Center);
        var hintRT = hint.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.1f, 0f);
        hintRT.anchorMax = new Vector2(0.9f, 0.05f);
        hintRT.offsetMin = hintRT.offsetMax = Vector2.zero;

        panel.SetActive(false);
    }

    void RefreshList()
    {
        if (listContainer == null) return;

        foreach (Transform child in listContainer)
            Destroy(child.gameObject);

        FishData[] allFish = FishSpawner.Instance != null ? FishSpawner.Instance.fishPool : null;
        if (allFish == null || allFish.Length == 0) return;

        int discovered = 0;
        foreach (var fish in allFish)
            if (FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(fish.fishName))
                discovered++;

        titleLabel.text = "Fish Compendium  " + discovered + " / " + allFish.Length;

        bool alternate = false;
        foreach (var fish in allFish)
        {
            bool known = FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(fish.fishName);

            var row = new GameObject("Row_" + fish.fishName);
            row.transform.SetParent(listContainer, false);
            var rowImg = row.AddComponent<Image>();
            rowImg.color = alternate
                ? new Color(0.07f, 0.1f, 0.19f, 1f)
                : new Color(0.05f, 0.07f, 0.14f, 1f);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 14f;

            if (known)
            {
                Color nameColor = RarityColor(fish.rarity);
                string rarityLabel = fish.rarity == 3 ? "Legendary" : fish.rarity == 2 ? "Uncommon" : "Common";
                string diffLabel = new string('*', fish.difficulty) + new string('-', 5 - fish.difficulty);

                MakeColumnLabel(row.transform, fish.fishName,         0.03f, 0.42f, nameColor);
                MakeColumnLabel(row.transform, rarityLabel,           0.42f, 0.62f, nameColor);
                MakeColumnLabel(row.transform, diffLabel,             0.62f, 0.82f, DifficultyColor(fish.difficulty));
                MakeColumnLabel(row.transform, fish.scoreValue.ToString(), 0.82f, 1.00f, new Color(1f, 0.85f, 0.3f));
            }
            else
            {
                Color dim = new Color(0.35f, 0.35f, 0.4f);
                MakeColumnLabel(row.transform, "???",  0.03f, 0.42f, dim);
                MakeColumnLabel(row.transform, "???",  0.42f, 0.62f, dim);
                MakeColumnLabel(row.transform, "???",  0.62f, 0.82f, dim);
                MakeColumnLabel(row.transform, "???",  0.82f, 1.00f, dim);
            }

            alternate = !alternate;
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    static TMP_Text MakeLabel(Transform parent, string text, float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }

    static void MakeColumnLabel(Transform parent, string text, float xMin, float xMax, Color color)
    {
        var label = MakeLabel(parent, text, 7f, color, TextAlignmentOptions.Left);
        var rt = label.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 0f);
        rt.anchorMax = new Vector2(xMax, 1f);
        rt.offsetMin = new Vector2(2f, 0f);
        rt.offsetMax = Vector2.zero;
    }

    static Color RarityColor(int rarity)
    {
        switch (rarity)
        {
            case 3: return new Color(1f,   0.75f, 0.1f);
            case 2: return new Color(0.4f, 0.8f,  0.4f);
            default: return new Color(0.85f, 0.85f, 0.85f);
        }
    }

    static Color DifficultyColor(int difficulty)
    {
        if (difficulty <= 2) return new Color(0.4f, 0.9f, 0.4f);
        if (difficulty == 3) return new Color(1f,   0.8f, 0.2f);
        return new Color(1f, 0.35f, 0.35f);
    }
}
