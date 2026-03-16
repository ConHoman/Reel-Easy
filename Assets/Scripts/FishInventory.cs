using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishInventory : MonoBehaviour
{
    public static FishInventory Instance;

    // Tracks count of each species+flavor combo caught this run, keyed by DisplayName
    private readonly Dictionary<string, (CaughtFish caught, int count)> caught =
        new Dictionary<string, (CaughtFish, int)>();

    private GameObject panel;
    private Transform listContainer;
    private TMP_Text titleLabel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    public void AddFish(CaughtFish cf)
    {
        if (cf.data == null) return;
        string key = cf.DisplayName;
        if (caught.ContainsKey(key))
            caught[key] = (cf, caught[key].count + 1);
        else
            caught[key] = (cf, 1);
        RefreshList();
    }

    public void ResetInventory()
    {
        caught.Clear();
        RefreshList();
    }

    // ── UI Construction ────────────────────────────────────────────────────────

    void BuildUI()
    {
        Canvas canvas = UICanvas.Get();

        // Backdrop
        panel = new GameObject("FishInventory");
        panel.transform.SetParent(canvas.transform, false);
        panel.AddComponent<Image>().color = new Color(0.05f, 0.07f, 0.12f, 0.96f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.1f, 0.08f);
        panelRT.anchorMax = new Vector2(0.9f, 0.92f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        // Title bar
        var titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(panel.transform, false);
        titleBar.AddComponent<Image>().color = new Color(0.08f, 0.11f, 0.2f, 1f);
        var titleBarRT = titleBar.GetComponent<RectTransform>();
        titleBarRT.anchorMin = new Vector2(0f, 0.88f);
        titleBarRT.anchorMax = Vector2.one;
        titleBarRT.offsetMin = titleBarRT.offsetMax = Vector2.zero;

        titleLabel = MakeLabel(titleBar.transform, "Fish Caught — 0", 9f, Color.white, TextAlignmentOptions.Center);
        var titleLabelRT = titleLabel.GetComponent<RectTransform>();
        titleLabelRT.anchorMin = Vector2.zero;
        titleLabelRT.anchorMax = Vector2.one;
        titleLabelRT.offsetMin = titleLabelRT.offsetMax = Vector2.zero;

        // Column headers
        var headers = new GameObject("Headers");
        headers.transform.SetParent(panel.transform, false);
        headers.AddComponent<Image>().color = new Color(0.1f, 0.14f, 0.24f, 1f);
        var headersRT = headers.GetComponent<RectTransform>();
        headersRT.anchorMin = new Vector2(0f, 0.79f);
        headersRT.anchorMax = new Vector2(1f, 0.88f);
        headersRT.offsetMin = headersRT.offsetMax = Vector2.zero;

        MakeColumnLabel(headers.transform, "FISH",    0.03f, 0.45f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "RARITY",  0.45f, 0.65f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "COUNT",   0.65f, 0.80f, new Color(0.6f, 0.6f, 0.7f));
        MakeColumnLabel(headers.transform, "SCORE",   0.80f, 1.00f, new Color(0.6f, 0.6f, 0.7f));

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

        // Mask
        var maskGO = new GameObject("Mask");
        maskGO.transform.SetParent(scrollGO.transform, false);
        var maskRT = maskGO.AddComponent<RectTransform>();
        maskRT.anchorMin = Vector2.zero;
        maskRT.anchorMax = Vector2.one;
        maskRT.offsetMin = maskRT.offsetMax = Vector2.zero;
        maskGO.AddComponent<Image>().color = Color.white;
        var mask = maskGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Content container
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

        // Hint label at bottom
        var hint = MakeLabel(panel.transform, "Press E to close", 6f, new Color(0.4f, 0.4f, 0.5f), TextAlignmentOptions.Center);
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

        int totalFish = 0;
        foreach (var entry in caught)
            totalFish += entry.Value.count;

        if (titleLabel != null)
            titleLabel.text = "Fish Caught — " + totalFish;

        bool alternate = false;
        foreach (var entry in caught)
        {
            CaughtFish cf   = entry.Value.caught;
            int        count = entry.Value.count;
            float flavorMult = FishFlavorData.Get(cf.flavor).scoreMultiplier;
            int   totalScore = Mathf.RoundToInt(cf.data.scoreValue * flavorMult) * count;

            var row = new GameObject("Row_" + cf.DisplayName);
            row.transform.SetParent(listContainer, false);
            row.AddComponent<Image>().color = alternate
                ? new Color(0.08f, 0.1f, 0.18f, 1f)
                : new Color(0.06f, 0.08f, 0.14f, 1f);
            row.AddComponent<LayoutElement>().preferredHeight = 14f;

            Color nameColor  = cf.flavor != FishFlavor.None
                ? FishFlavorData.LabelColor(cf.flavor)
                : RarityColor(cf.data.rarity);
            string rarityLabel = cf.data.rarity == 5 ? "Mythical"
                               : cf.data.rarity == 4 ? "Legendary"
                               : cf.data.rarity == 3 ? "Epic"
                               : cf.data.rarity == 2 ? "Uncommon"
                               :                       "Common";

            MakeColumnLabel(row.transform, cf.DisplayName,        0.03f, 0.45f, nameColor);
            MakeColumnLabel(row.transform, rarityLabel,           0.45f, 0.65f, RarityColor(cf.data.rarity));
            MakeColumnLabel(row.transform, "x" + count,          0.65f, 0.80f, Color.white);
            MakeColumnLabel(row.transform, totalScore.ToString(), 0.80f, 1.00f, new Color(1f, 0.85f, 0.3f));

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
            case 5: return new Color(1f,    0.2f,  0.8f);   // mythical  — hot pink
            case 4: return new Color(1f,    0.75f, 0.1f);   // legendary — gold
            case 3: return new Color(0.2f,  0.6f,  1.0f);   // epic      — azure blue
            case 2: return new Color(0.4f,  0.8f,  0.4f);   // uncommon  — green
            default: return new Color(0.85f, 0.85f, 0.85f); // common    — light grey
        }
    }
}
