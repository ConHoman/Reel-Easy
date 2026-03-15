using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Press Tab to open/close. Shows all fish with stats; undiscovered appear as ???.
public class FishCompendium : MonoBehaviour
{
    public static FishCompendium Instance;

    private GameObject panel;
    private Transform  listContainer;
    private TMP_Text   titleLabel;
    private FishData[] cachedFish;   // built once from FishDatabase

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

        MakeColumnLabel(headers.transform, "FISH",       0.03f, 0.48f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "DIFFICULTY", 0.48f, 0.72f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "SCORE",      0.72f, 1.00f, new Color(0.6f, 0.6f, 0.7f));

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

        // Use FishDatabase as the authoritative list so the compendium always shows
        // every fish regardless of whether FishSpawner is in the scene.
        if (cachedFish == null)
            cachedFish = FishDatabase.CreateAll();

        if (cachedFish == null || cachedFish.Length == 0) return;

        // Sort: rarity ascending (common first), then score ascending within group
        var sorted = new System.Collections.Generic.List<FishData>(cachedFish);
        sorted.Sort((a, b) => a.rarity != b.rarity ? a.rarity.CompareTo(b.rarity) : a.scoreValue.CompareTo(b.scoreValue));

        int discovered = 0;
        foreach (var fish in sorted)
            if (FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(fish.fishName))
                discovered++;

        titleLabel.text = "Fish Compendium  " + discovered + " / " + cachedFish.Length;

        int currentRarity = -1;
        bool alternate = false;

        foreach (var fish in sorted)
        {
            // Rarity section header
            if (fish.rarity != currentRarity)
            {
                currentRarity = fish.rarity;
                string[] rarityNames = { "", "Common", "Uncommon", "Legendary", "Mythical" };
                Color headerBg = currentRarity == 4
                    ? new Color(0.18f, 0.02f, 0.14f)
                    : currentRarity == 3
                        ? new Color(0.18f, 0.10f, 0.04f)
                        : currentRarity == 2
                            ? new Color(0.04f, 0.14f, 0.08f)
                            : new Color(0.08f, 0.09f, 0.16f);

                var hdrRow = new GameObject("Header_" + rarityNames[currentRarity]);
                hdrRow.transform.SetParent(listContainer, false);
                hdrRow.AddComponent<Image>().color = headerBg;
                hdrRow.AddComponent<LayoutElement>().preferredHeight = 13f;

                // Count fish in this rarity group that are discovered
                int groupTotal = 0, groupFound = 0;
                foreach (var f in sorted)
                {
                    if (f.rarity != currentRarity) continue;
                    groupTotal++;
                    if (FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(f.fishName))
                        groupFound++;
                }

                string hdrText = rarityNames[currentRarity].ToUpper() + "  " + groupFound + " / " + groupTotal;
                var hdrLbl = MakeLabel(hdrRow.transform, hdrText, 6f, RarityColor(currentRarity), TextAlignmentOptions.Left);
                var hRT = hdrLbl.GetComponent<RectTransform>();
                hRT.anchorMin = new Vector2(0.03f, 0f);
                hRT.anchorMax = Vector2.one;
                hRT.offsetMin = hRT.offsetMax = Vector2.zero;

                alternate = false;
            }

            bool known = FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(fish.fishName);

            var row = new GameObject("Row_" + fish.fishName);
            row.transform.SetParent(listContainer, false);
            row.AddComponent<Image>().color = alternate
                ? new Color(0.07f, 0.09f, 0.18f)
                : new Color(0.05f, 0.07f, 0.13f);
            row.AddComponent<LayoutElement>().preferredHeight = 13f;

            if (known)
            {
                string diffStr = new string('*', fish.difficulty) + new string('-', 5 - fish.difficulty);
                MakeColumnLabel(row.transform, fish.fishName,              0.03f, 0.48f, RarityColor(fish.rarity));
                MakeColumnLabel(row.transform, diffStr,                    0.48f, 0.72f, DifficultyColor(fish.difficulty));
                MakeColumnLabel(row.transform, fish.scoreValue.ToString(), 0.72f, 1.00f, new Color(1f, 0.85f, 0.3f));
            }
            else
            {
                Color dim = new Color(0.32f, 0.32f, 0.38f);
                MakeColumnLabel(row.transform, "???", 0.03f, 0.48f, dim);
                MakeColumnLabel(row.transform, "???", 0.48f, 0.72f, dim);
                MakeColumnLabel(row.transform, "???", 0.72f, 1.00f, dim);
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
            case 4: return new Color(1f,   0.2f,  0.8f);  // mythical — hot pink
            case 3: return new Color(1f,   0.75f, 0.1f);  // legendary — gold
            case 2: return new Color(0.4f, 0.8f,  0.4f);  // uncommon  — green
            default: return new Color(0.85f, 0.85f, 0.85f); // common  — grey
        }
    }

    static Color DifficultyColor(int difficulty)
    {
        if (difficulty <= 2) return new Color(0.4f, 0.9f, 0.4f);
        if (difficulty == 3) return new Color(1f,   0.8f, 0.2f);
        return new Color(1f, 0.35f, 0.35f);
    }
}
