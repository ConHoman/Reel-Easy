using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Press Tab to open/close. Shows all fish; undiscovered appear as ???.
// Tab bar at top switches between Normal and each flavor variant.
// Hovering a discovered fish row shows its flavor text.
public class FishCompendium : MonoBehaviour
{
    public static FishCompendium Instance;

    // -1 = Normal tab, 0-5 = index into FishFlavorData.AllFlavors
    private int currentTabIndex = -1;

    private GameObject panel;
    private Transform  listContainer;
    private TMP_Text   titleLabel;
    private TMP_Text   tooltipLabel;
    private FishData[] cachedFish;
    private readonly List<TMP_Text> tabLabels = new List<TMP_Text>();

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

        // ── Title bar ─────────────────────────────────────────────────────────
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

        // ── Tab bar (Normal + 6 flavor tabs) ─────────────────────────────────
        var tabBar = new GameObject("TabBar");
        tabBar.transform.SetParent(panel.transform, false);
        tabBar.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.18f, 1f);
        var tabBarRT = tabBar.GetComponent<RectTransform>();
        tabBarRT.anchorMin = new Vector2(0f, 0.82f);
        tabBarRT.anchorMax = new Vector2(1f, 0.88f);
        tabBarRT.offsetMin = tabBarRT.offsetMax = Vector2.zero;

        // 7 tabs: Normal + 6 flavors
        string[] tabNames  = { "Normal", "Albino", "Shiny", "Ancient", "Giant", "Golden", "Cursed" };
        float    tabWidth  = 1f / tabNames.Length;
        for (int t = 0; t < tabNames.Length; t++)
        {
            int capturedIndex = t - 1; // -1 = Normal, 0-5 = flavor index
            float xMin = t * tabWidth;
            float xMax = xMin + tabWidth;

            var tabGO  = new GameObject("Tab_" + tabNames[t]);
            tabGO.transform.SetParent(tabBar.transform, false);
            var tabBtn = tabGO.AddComponent<Button>();
            tabBtn.transition = Selectable.Transition.None;
            var tabImg = tabGO.AddComponent<Image>();
            tabImg.color = new Color(0.07f, 0.1f, 0.22f, 1f);
            var tabRT = tabGO.GetComponent<RectTransform>();
            tabRT.anchorMin = new Vector2(xMin, 0f);
            tabRT.anchorMax = new Vector2(xMax, 1f);
            tabRT.offsetMin = new Vector2(1f,  1f);
            tabRT.offsetMax = new Vector2(-1f, -1f);

            Color labelColor = t == 0 ? Color.white : FishFlavorData.LabelColor(FishFlavorData.AllFlavors[t - 1]);
            var lbl = MakeLabel(tabGO.transform, tabNames[t], 5.5f, labelColor, TextAlignmentOptions.Center);
            lbl.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            lbl.GetComponent<RectTransform>().anchorMax = Vector2.one;
            lbl.GetComponent<RectTransform>().offsetMin = lbl.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            tabLabels.Add(lbl);

            tabBtn.onClick.AddListener(() =>
            {
                currentTabIndex = capturedIndex;
                RefreshTabHighlights();
                RefreshList();
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);
            });
        }

        RefreshTabHighlights();

        // ── Column headers ────────────────────────────────────────────────────
        var headers = new GameObject("Headers");
        headers.transform.SetParent(panel.transform, false);
        headers.AddComponent<Image>().color = new Color(0.09f, 0.13f, 0.26f, 1f);
        var headersRT = headers.GetComponent<RectTransform>();
        headersRT.anchorMin = new Vector2(0f, 0.75f);
        headersRT.anchorMax = new Vector2(1f, 0.82f);
        headersRT.offsetMin = headersRT.offsetMax = Vector2.zero;

        MakeColumnLabel(headers.transform, "FISH",       0.03f, 0.48f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "DIFFICULTY", 0.48f, 0.72f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "SCORE",      0.72f, 1.00f, new Color(0.6f, 0.6f, 0.7f));

        // ── Scroll area ───────────────────────────────────────────────────────
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(panel.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0.13f);
        scrollRT.anchorMax = new Vector2(1f, 0.75f);
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
        maskGO.AddComponent<Mask>().showMaskGraphic = false;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(maskGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = Vector2.one;
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;

        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing             = 1f;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight     = true;

        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = maskRT;
        scrollRect.content  = contentRT;
        listContainer = contentGO.transform;

        // ── Flavor text tooltip area ──────────────────────────────────────────
        var tooltipBG = new GameObject("TooltipBG");
        tooltipBG.transform.SetParent(panel.transform, false);
        tooltipBG.AddComponent<Image>().color = new Color(0.06f, 0.09f, 0.2f, 1f);
        var tooltipBGRT = tooltipBG.GetComponent<RectTransform>();
        tooltipBGRT.anchorMin = new Vector2(0f, 0.06f);
        tooltipBGRT.anchorMax = new Vector2(1f, 0.13f);
        tooltipBGRT.offsetMin = tooltipBGRT.offsetMax = Vector2.zero;

        tooltipLabel = MakeLabel(tooltipBG.transform, "", 6f, new Color(0.75f, 0.8f, 1f), TextAlignmentOptions.Center);
        tooltipLabel.fontStyle = FontStyles.Italic;
        var tlpRT = tooltipLabel.GetComponent<RectTransform>();
        tlpRT.anchorMin = new Vector2(0.03f, 0f);
        tlpRT.anchorMax = Vector2.one;
        tlpRT.offsetMin = tlpRT.offsetMax = Vector2.zero;

        // ── Hint ─────────────────────────────────────────────────────────────
        var hint = MakeLabel(panel.transform, "Tab to close", 6f, new Color(0.4f, 0.4f, 0.5f), TextAlignmentOptions.Center);
        var hintRT = hint.GetComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(0.1f, 0f);
        hintRT.anchorMax = new Vector2(0.9f, 0.06f);
        hintRT.offsetMin = hintRT.offsetMax = Vector2.zero;

        panel.SetActive(false);
    }

    // ── List Refresh ───────────────────────────────────────────────────────────

    void RefreshTabHighlights()
    {
        for (int i = 0; i < tabLabels.Count; i++)
        {
            bool active = (i - 1) == currentTabIndex;
            var img = tabLabels[i].transform.parent.GetComponent<Image>();
            if (img != null)
                img.color = active
                    ? new Color(0.12f, 0.18f, 0.38f, 1f)
                    : new Color(0.07f, 0.10f, 0.22f, 1f);
            tabLabels[i].fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    void RefreshList()
    {
        if (listContainer == null) return;
        if (tooltipLabel != null) tooltipLabel.text = "";

        foreach (Transform child in listContainer)
            Destroy(child.gameObject);

        if (cachedFish == null)
            cachedFish = FishDatabase.CreateAll();
        if (cachedFish == null || cachedFish.Length == 0) return;

        var sorted = new List<FishData>(cachedFish);
        sorted.Sort((a, b) => a.rarity != b.rarity ? a.rarity.CompareTo(b.rarity) : a.scoreValue.CompareTo(b.scoreValue));

        if (currentTabIndex == -1)
            BuildNormalTab(sorted);
        else
            BuildFlavorTab(sorted, FishFlavorData.AllFlavors[currentTabIndex]);
    }

    void BuildNormalTab(List<FishData> sorted)
    {
        int totalDiscovered = 0;
        foreach (var f in sorted)
            if (FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(f.fishName))
                totalDiscovered++;
        titleLabel.text = "Fish Compendium  " + totalDiscovered + " / " + cachedFish.Length;

        int  currentRarity = -1;
        bool alternate     = false;

        foreach (var fish in sorted)
        {
            if (fish.rarity != currentRarity)
            {
                currentRarity = fish.rarity;
                AddRarityHeader(currentRarity, sorted, false, FishFlavor.None);
                alternate = false;
            }

            bool known = FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(fish.fishName);
            var row = MakeRow(alternate);

            if (known)
            {
                string diffStr = new string('*', fish.difficulty) + new string('-', 5 - fish.difficulty);
                MakeColumnLabel(row.transform, fish.fishName,              0.03f, 0.48f, RarityColor(fish.rarity));
                MakeColumnLabel(row.transform, diffStr,                    0.48f, 0.72f, DifficultyColor(fish.difficulty));
                MakeColumnLabel(row.transform, fish.scoreValue.ToString(), 0.72f, 1.00f, new Color(1f, 0.85f, 0.3f));
                AddTooltipTriggers(row, fish.flavorText);
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

    void BuildFlavorTab(List<FishData> sorted, FishFlavor flavor)
    {
        var info = FishFlavorData.Get(flavor);
        int totalDiscovered = 0;
        foreach (var f in sorted)
            if (FishJournal.Instance != null && FishJournal.Instance.IsDiscoveredFlavor(f.fishName, flavor))
                totalDiscovered++;
        titleLabel.text = FishFlavorData.Get(flavor).displayName + " Variants  " + totalDiscovered + " / " + cachedFish.Length;

        int  currentRarity = -1;
        bool alternate     = false;

        foreach (var fish in sorted)
        {
            if (fish.rarity != currentRarity)
            {
                currentRarity = fish.rarity;
                AddRarityHeader(currentRarity, sorted, true, flavor);
                alternate = false;
            }

            bool knownSpecies = FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(fish.fishName);
            bool knownFlavor  = FishJournal.Instance != null && FishJournal.Instance.IsDiscoveredFlavor(fish.fishName, flavor);
            var row = MakeRow(alternate);

            if (knownFlavor)
            {
                int   flavorScore = Mathf.RoundToInt(fish.scoreValue * info.scoreMultiplier);
                int   flavorDiff  = Mathf.Min(5, fish.difficulty + info.difficultyDelta);
                string diffStr    = new string('*', flavorDiff) + new string('-', 5 - flavorDiff);
                string displayName = info.displayName + " " + fish.fishName;

                MakeColumnLabel(row.transform, displayName,           0.03f, 0.48f, FishFlavorData.LabelColor(flavor));
                MakeColumnLabel(row.transform, diffStr,               0.48f, 0.72f, DifficultyColor(flavorDiff));
                MakeColumnLabel(row.transform, flavorScore.ToString(), 0.72f, 1.00f, new Color(1f, 0.85f, 0.3f));
                AddTooltipTriggers(row, fish.flavorText);
            }
            else if (knownSpecies)
            {
                // Species known but flavor variant not caught yet — show name dimmed
                Color dim = new Color(0.45f, 0.45f, 0.5f);
                MakeColumnLabel(row.transform, fish.fishName, 0.03f, 0.48f, dim);
                MakeColumnLabel(row.transform, "???",         0.48f, 0.72f, dim);
                MakeColumnLabel(row.transform, "???",         0.72f, 1.00f, dim);
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

    void AddRarityHeader(int rarity, List<FishData> sorted, bool flavorTab, FishFlavor flavor)
    {
        string[] rarityNames = { "", "Common", "Uncommon", "Epic", "Legendary", "Mythical" };
        Color headerBg = rarity == 5 ? new Color(0.18f, 0.02f, 0.14f)  // mythical
                       : rarity == 4 ? new Color(0.18f, 0.10f, 0.04f)  // legendary
                       : rarity == 3 ? new Color(0.04f, 0.06f, 0.18f)  // epic
                       : rarity == 2 ? new Color(0.04f, 0.14f, 0.08f)  // uncommon
                                     : new Color(0.08f, 0.09f, 0.16f); // common

        var hdrRow = new GameObject("Header_" + rarityNames[rarity]);
        hdrRow.transform.SetParent(listContainer, false);
        hdrRow.AddComponent<Image>().color = headerBg;
        hdrRow.AddComponent<LayoutElement>().preferredHeight = 13f;

        int groupTotal = 0, groupFound = 0;
        foreach (var f in sorted)
        {
            if (f.rarity != rarity) continue;
            groupTotal++;
            bool found = flavorTab
                ? (FishJournal.Instance != null && FishJournal.Instance.IsDiscoveredFlavor(f.fishName, flavor))
                : (FishJournal.Instance != null && FishJournal.Instance.IsDiscovered(f.fishName));
            if (found) groupFound++;
        }

        string hdrText = rarityNames[rarity].ToUpper() + "  " + groupFound + " / " + groupTotal;
        var hdrLbl = MakeLabel(hdrRow.transform, hdrText, 6f, RarityColor(rarity), TextAlignmentOptions.Left);
        var hRT = hdrLbl.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0.03f, 0f);
        hRT.anchorMax = Vector2.one;
        hRT.offsetMin = hRT.offsetMax = Vector2.zero;
    }

    GameObject MakeRow(bool alternate)
    {
        var row = new GameObject("Row");
        row.transform.SetParent(listContainer, false);
        row.AddComponent<Image>().color = alternate
            ? new Color(0.07f, 0.09f, 0.18f)
            : new Color(0.05f, 0.07f, 0.13f);
        row.AddComponent<LayoutElement>().preferredHeight = 13f;
        return row;
    }

    void AddTooltipTriggers(GameObject row, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        string captured = text;

        var trigger = row.AddComponent<EventTrigger>();

        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((_) => { if (tooltipLabel != null) tooltipLabel.text = captured; });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((_) => { if (tooltipLabel != null) tooltipLabel.text = ""; });
        trigger.triggers.Add(exitEntry);

        // Without this, EventTrigger swallows scroll events and the list can't be scrolled while hovering
        ScrollRect parentScroll = row.GetComponentInParent<ScrollRect>();
        if (parentScroll != null)
        {
            var scrollEntry = new EventTrigger.Entry { eventID = EventTriggerType.Scroll };
            scrollEntry.callback.AddListener((data) => parentScroll.OnScroll((PointerEventData)data));
            trigger.triggers.Add(scrollEntry);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    static TMP_Text MakeLabel(Transform parent, string text, float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
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
            case 5:  return new Color(1f,    0.2f,  0.8f);   // mythical
            case 4:  return new Color(1f,    0.75f, 0.1f);   // legendary
            case 3:  return new Color(0.2f,  0.6f,  1.0f);   // epic
            case 2:  return new Color(0.4f,  0.8f,  0.4f);   // uncommon
            default: return new Color(0.85f, 0.85f, 0.85f);  // common
        }
    }

    static Color DifficultyColor(int difficulty)
    {
        if (difficulty <= 2) return new Color(0.4f, 0.9f, 0.4f);
        if (difficulty == 3) return new Color(1f,   0.8f, 0.2f);
        return new Color(1f, 0.35f, 0.35f);
    }
}
