using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Press Q to open/close. Shows all active perks with stacks, effects, and category colours.
public class PerkViewerUI : MonoBehaviour
{
    public static PerkViewerUI Instance;

    private GameObject viewerPanel;
    private Transform  listContent;   // VerticalLayoutGroup content inside the ScrollRect

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
            new GameObject("PerkViewerUI").AddComponent<PerkViewerUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool open = !viewerPanel.activeSelf;
            viewerPanel.SetActive(open);
            if (open) Populate();
        }
    }

    // ── UI build (panel + scrollable list) ────────────────────────────────────

    void BuildUI()
    {
        Canvas canvas = UICanvas.Get();

        // ── Outer panel ──────────────────────────────────────────────────────
        viewerPanel = new GameObject("PerkViewerPanel");
        viewerPanel.transform.SetParent(canvas.transform, false);
        viewerPanel.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.13f, 0.95f);
        var panelRT = viewerPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.10f, 0.06f);
        panelRT.anchorMax = new Vector2(0.90f, 0.94f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        Transform p = viewerPanel.transform;

        // ── Title bar (top 10%) ───────────────────────────────────────────────
        var titleBG = PVRect(p, "TitleBG", new Color(0.08f, 0.11f, 0.24f));
        titleBG.anchorMin = new Vector2(0f, 0.90f);
        titleBG.anchorMax = Vector2.one;
        titleBG.offsetMin = titleBG.offsetMax = Vector2.zero;

        var titleLbl = PVLabel(titleBG, "Active Perks", 8.5f, Color.white);
        var tlRT = titleLbl.GetComponent<RectTransform>();
        tlRT.anchorMin = new Vector2(0.02f, 0f); tlRT.anchorMax = new Vector2(0.72f, 1f);
        tlRT.offsetMin = tlRT.offsetMax = Vector2.zero;
        titleLbl.alignment = TextAlignmentOptions.Left;
        titleLbl.fontStyle = FontStyles.Bold;

        var closeLbl = PVLabel(titleBG, "[Q] close", 5.5f, new Color(0.6f, 0.6f, 0.8f));
        var clRT = closeLbl.GetComponent<RectTransform>();
        clRT.anchorMin = new Vector2(0.74f, 0f); clRT.anchorMax = Vector2.one;
        clRT.offsetMin = clRT.offsetMax = Vector2.zero;
        closeLbl.alignment = TextAlignmentOptions.Right;

        // ── ScrollRect (y = 0.01 – 0.89) ─────────────────────────────────────
        var scrollGO = new GameObject("ScrollArea");
        scrollGO.transform.SetParent(p, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0.01f);
        scrollRT.anchorMax = new Vector2(1f, 0.89f);
        scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.scrollSensitivity = 20f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport (masked)
        var vpGO = new GameObject("Viewport");
        vpGO.transform.SetParent(scrollGO.transform, false);
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        var vpRT = vpGO.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;

        // Content (grows downward via VLG + CSF)
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(4, 4, 4, 4);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true;
        vlg.childControlWidth  = true;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot      = new Vector2(0.5f, 1f);
        contentRT.sizeDelta  = Vector2.zero;

        scrollRect.content  = contentRT;
        scrollRect.viewport = vpRT;
        listContent = contentGO.transform;

        viewerPanel.SetActive(false);
    }

    // ── Populate list ─────────────────────────────────────────────────────────

    void Populate()
    {
        // Clear old rows
        foreach (Transform c in listContent) Destroy(c.gameObject);

        if (PerkManager.Instance == null || PerkManager.Instance.ActivePerks.Count == 0)
        {
            AddEmptyRow("No perks yet.");
            return;
        }

        // Count stacks per type
        var counts = new Dictionary<PerkType, int>();
        foreach (var p in PerkManager.Instance.ActivePerks)
        {
            if (!counts.TryGetValue(p, out int n)) counts[p] = 1;
            else counts[p] = n + 1;
        }

        // Build ordered list matching AllPerks order, grouped by category
        PerkCategory? lastCat = null;
        foreach (var def in PerkManager.AllPerks)
        {
            if (!counts.TryGetValue(def.type, out int stacks)) continue;

            // Category header when category changes
            if (def.category != lastCat)
            {
                lastCat = def.category;
                AddCategoryHeader(def.category);
            }

            AddPerkRow(def, stacks);
        }
    }

    void AddCategoryHeader(PerkCategory cat)
    {
        var go = new GameObject("CatHdr");
        go.transform.SetParent(listContent, false);

        go.AddComponent<Image>().color = new Color(
            CategoryColor(cat).r * 0.35f,
            CategoryColor(cat).g * 0.35f,
            CategoryColor(cat).b * 0.35f, 1f);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 11f;

        var txt = PVLabel(go.transform, cat.ToString().ToUpper(), 5.5f, CategoryColor(cat));
        txt.fontStyle  = FontStyles.Bold;
        txt.alignment  = TextAlignmentOptions.Left;
        var tRT = txt.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0.02f, 0f); tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
    }

    void AddPerkRow(PerkDefinition def, int stacks)
    {
        var rowGO = new GameObject("Row_" + def.displayName);
        rowGO.transform.SetParent(listContent, false);

        // Alternating row background
        rowGO.AddComponent<Image>().color = new Color(0.07f, 0.09f, 0.17f, 1f);

        // Horizontal layout: [colour bar] [text block]
        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing            = 4f;
        hlg.padding            = new RectOffset(0, 4, 2, 2);
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth  = false;
        hlg.childControlHeight = true;
        hlg.childControlWidth  = true;

        // Row auto-sizes to its text
        rowGO.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        // Left colour bar
        var barGO = new GameObject("Bar");
        barGO.transform.SetParent(rowGO.transform, false);
        barGO.AddComponent<Image>().color = CategoryColor(def.category);
        barGO.AddComponent<LayoutElement>().preferredWidth = 4f;

        // Text content
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(rowGO.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize          = 5.5f;
        tmp.color             = Color.white;
        tmp.enableWordWrapping = true;
        tmp.text              = BuildRowText(def, stacks);
        textGO.AddComponent<LayoutElement>().flexibleWidth = 1f;
    }

    void AddEmptyRow(string message)
    {
        var go = new GameObject("Empty");
        go.transform.SetParent(listContent, false);
        go.AddComponent<LayoutElement>().preferredHeight = 20f;
        var lbl = PVLabel(go.transform, message, 6f, new Color(0.5f, 0.5f, 0.7f));
        lbl.alignment = TextAlignmentOptions.Center;
    }

    static string BuildRowText(PerkDefinition def, int stacks)
    {
        string stackBadge = stacks > 1
            ? $"  <color=#FFD700><b>×{stacks}</b></color>"
            : "";

        string catHex = CategoryHex(def.category);
        string line1  = $"<b>{def.displayName}</b>{stackBadge}  <color={catHex}>[{def.category}]</color>";

        string line2 = $"<color=#55EE77>+ {def.upside}</color>";
        if (!string.IsNullOrEmpty(def.downside))
            line2 += $"    <color=#FF7777>- {def.downside}</color>";

        return line1 + "\n" + line2;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Color CategoryColor(PerkCategory cat)
    {
        switch (cat)
        {
            case PerkCategory.Safety:   return new Color(0.40f, 0.65f, 1.00f);
            case PerkCategory.Fishing:  return new Color(0.27f, 0.90f, 0.80f);
            case PerkCategory.Scoring:  return new Color(1.00f, 0.87f, 0.20f);
            case PerkCategory.Minigame: return new Color(0.80f, 0.53f, 1.00f);
            default:                    return Color.white;
        }
    }

    static string CategoryHex(PerkCategory cat)
    {
        switch (cat)
        {
            case PerkCategory.Safety:   return "#66AAFF";
            case PerkCategory.Fishing:  return "#44DDCC";
            case PerkCategory.Scoring:  return "#FFDD33";
            case PerkCategory.Minigame: return "#CC88FF";
            default:                    return "#FFFFFF";
        }
    }

    static RectTransform PVRect(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    static TMP_Text PVLabel(Transform parent, string text, float size, Color color)
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
}
