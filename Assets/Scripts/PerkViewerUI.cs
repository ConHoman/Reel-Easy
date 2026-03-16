using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Press Q to open/close. Shows all active perks as a formatted text list.
public class PerkViewerUI : MonoBehaviour
{
    public static PerkViewerUI Instance;

    private GameObject viewerPanel;
    private TMP_Text   listText;
    private TMP_Text   statsText;

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

    // ── Build ─────────────────────────────────────────────────────────────────

    void BuildUI()
    {
        Canvas canvas = UICanvas.Get();

        // Panel backdrop
        viewerPanel = new GameObject("PerkViewerPanel");
        viewerPanel.transform.SetParent(canvas.transform, false);
        viewerPanel.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.13f, 0.96f);
        var panelRT = viewerPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.10f, 0.06f);
        panelRT.anchorMax = new Vector2(0.90f, 0.94f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        // Title bar
        var titleBarGO = new GameObject("TitleBar");
        titleBarGO.transform.SetParent(viewerPanel.transform, false);
        titleBarGO.AddComponent<Image>().color = new Color(0.08f, 0.11f, 0.24f);
        var tbRT = titleBarGO.GetComponent<RectTransform>();
        tbRT.anchorMin = new Vector2(0f, 0.90f);
        tbRT.anchorMax = Vector2.one;
        tbRT.offsetMin = tbRT.offsetMax = Vector2.zero;

        var titleTxt = new GameObject("TitleTxt");
        titleTxt.transform.SetParent(titleBarGO.transform, false);
        var tt = titleTxt.AddComponent<TextMeshProUGUI>();
        tt.text      = "Active Perks";
        tt.fontSize  = 8.5f;
        tt.color     = Color.white;
        tt.alignment = TextAlignmentOptions.Left;
        tt.fontStyle = FontStyles.Bold;
        var ttRT = titleTxt.GetComponent<RectTransform>();
        ttRT.anchorMin = new Vector2(0.02f, 0f);
        ttRT.anchorMax = new Vector2(0.70f, 1f);
        ttRT.offsetMin = ttRT.offsetMax = Vector2.zero;

        var closeTxt = new GameObject("CloseTxt");
        closeTxt.transform.SetParent(titleBarGO.transform, false);
        var ct = closeTxt.AddComponent<TextMeshProUGUI>();
        ct.text      = "[Q] close";
        ct.fontSize  = 5.5f;
        ct.color     = new Color(0.6f, 0.6f, 0.8f);
        ct.alignment = TextAlignmentOptions.Right;
        var ctRT = closeTxt.GetComponent<RectTransform>();
        ctRT.anchorMin = new Vector2(0.72f, 0f);
        ctRT.anchorMax = new Vector2(0.97f, 1f);
        ctRT.offsetMin = ctRT.offsetMax = Vector2.zero;

        // Left column: active perk list — word wrap keeps text within column bounds
        var listGO = new GameObject("PerkListText");
        listGO.transform.SetParent(viewerPanel.transform, false);
        listText = listGO.AddComponent<TextMeshProUGUI>();
        listText.fontSize           = 5.5f;
        listText.color              = Color.white;
        listText.enableWordWrapping = true;
        listText.overflowMode       = TextOverflowModes.Overflow;
        var listRT = listGO.GetComponent<RectTransform>();
        listRT.anchorMin = new Vector2(0.02f, 0.02f);
        listRT.anchorMax = new Vector2(0.58f, 0.89f);
        listRT.offsetMin = listRT.offsetMax = Vector2.zero;

        // Divider between columns
        var divGO = new GameObject("ColumnDivider");
        divGO.transform.SetParent(viewerPanel.transform, false);
        divGO.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.10f);
        var divRT = divGO.GetComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0.60f, 0.02f);
        divRT.anchorMax = new Vector2(0.61f, 0.89f);
        divRT.offsetMin = divRT.offsetMax = Vector2.zero;

        // Right column: stat spread
        var statsGO = new GameObject("StatsText");
        statsGO.transform.SetParent(viewerPanel.transform, false);
        statsText = statsGO.AddComponent<TextMeshProUGUI>();
        statsText.fontSize           = 5f;
        statsText.color              = Color.white;
        statsText.enableWordWrapping = false;
        statsText.overflowMode       = TextOverflowModes.Truncate;
        var statsRT = statsGO.GetComponent<RectTransform>();
        statsRT.anchorMin = new Vector2(0.62f, 0.02f);
        statsRT.anchorMax = new Vector2(0.98f, 0.89f);
        statsRT.offsetMin = statsRT.offsetMax = Vector2.zero;

        viewerPanel.SetActive(false);
    }

    // ── Populate ──────────────────────────────────────────────────────────────

    void Populate()
    {
        if (listText == null) return;

        if (PerkManager.Instance == null || PerkManager.Instance.ActivePerks.Count == 0)
        {
            listText.text = "\n<color=#8888BB>  No perks yet.</color>";
            if (statsText != null) statsText.text = "";
            return;
        }

        // Count stacks per type
        var counts = new Dictionary<PerkType, int>();
        foreach (var p in PerkManager.Instance.ActivePerks)
        {
            if (!counts.ContainsKey(p)) counts[p] = 0;
            counts[p]++;
        }

        var sb = new StringBuilder();
        PerkCategory? lastCat = null;

        foreach (var def in PerkManager.AllPerks)
        {
            if (!counts.TryGetValue(def.type, out int stacks)) continue;

            // Category header
            if (def.category != lastCat)
            {
                if (lastCat != null) sb.AppendLine();
                lastCat = def.category;
                string catHex = CategoryHex(def.category);
                sb.AppendLine($"<color={catHex}><b>-- {def.category.ToString().ToUpper()} --</b></color>");
            }

            // Perk row
            string stackStr = stacks > 1 ? $" <color=#FFD700><b>x{stacks}</b></color>" : "";
            sb.Append($"  <b>{def.displayName}</b>{stackStr}");
            sb.Append($"   <color=#55EE77>{def.upside}</color>");
            if (!string.IsNullOrEmpty(def.downside))
                sb.Append($"   <color=#FF7777>{def.downside}</color>");
            sb.AppendLine();
        }

        listText.text = sb.ToString();
        PopulateStats();
    }

    // ── Stats ─────────────────────────────────────────────────────────────────

    void PopulateStats()
    {
        if (statsText == null) return;
        var pm = PerkManager.Instance;
        if (pm == null) { statsText.text = ""; return; }

        var sb = new StringBuilder();

        // ── Fishing ──────────────────────────────────────────────────────────
        sb.AppendLine("<color=#AAAADD><b>STATS</b></color>");
        sb.AppendLine("<color=#44DDCC><b>Fishing</b></color>");
        Stat(sb, "Spawn radius",  $"x{pm.SpawnRadiusMultiplier:F2}",    pm.SpawnRadiusMultiplier    != 1f, true);
        Stat(sb, "Max fish/cast", pm.SpawnCountBonus > 0 ? $"+{pm.SpawnCountBonus}" : "+0",
                                                                          pm.SpawnCountBonus          != 0,  true);
        Stat(sb, "Steer time",    $"x{pm.SteerDurationMultiplier:F2}",  pm.SteerDurationMultiplier  != 1f, true);
        Stat(sb, "Bobber speed",  $"x{pm.TipSpeedMultiplier:F2}",       pm.TipSpeedMultiplier       != 1f, true);
        Stat(sb, "Hitbox size",   $"x{pm.HitboxMultiplier:F2}",         pm.HitboxMultiplier         != 1f, true);

        // ── Rarity ───────────────────────────────────────────────────────────
        sb.AppendLine("<color=#44DDCC><b>Rarity</b></color>");

        string[] rarityNames = { "Common", "Uncommon", "Epic", "Legendary" };
        for (int r = 1; r <= 4; r++)
        {
            int bonus = pm.RaritySpawnBonus(r);
            float flee = pm.RarityFleeMultiplier(r);
            string spawnVal = bonus > 0 ? $"+{bonus}" : "+0";
            string fleeVal  = $"x{flee:F2}";
            bool spawnMod   = bonus  != 0;
            bool fleeMod    = Mathf.Abs(flee - 1f) > 0.01f;
            // spawn
            Stat(sb, $"{rarityNames[r-1]} spawn", spawnVal, spawnMod, true);
            // flee — only show if a perk modified it
            if (fleeMod)
                Stat(sb, $"{rarityNames[r-1]} flee", fleeVal, true, false);
        }

        float mg = pm.MythicalGateBonus;
        Stat(sb, "Mythical gate", $"x{mg:F2}", Mathf.Abs(mg - 1f) > 0.01f, true);

        // ── Variants ─────────────────────────────────────────────────────────
        sb.AppendLine("<color=#44DDCC><b>Variants</b></color>");

        FishFlavor[] flavors      = { FishFlavor.Albino, FishFlavor.Shiny, FishFlavor.Ancient,
                                      FishFlavor.Giant,  FishFlavor.Golden, FishFlavor.Cursed };
        string[]     flavorNames  = { "Albino", "Shiny", "Ancient", "Giant", "Golden", "Cursed" };

        for (int i = 0; i < flavors.Length; i++)
        {
            float spawn = pm.FlavorSpawnMultiplier(flavors[i]);
            float flee  = pm.FlavorFleeMultiplier(flavors[i]);
            bool spawnMod = Mathf.Abs(spawn - 1f) > 0.01f;
            bool fleeMod  = Mathf.Abs(flee  - 1f) > 0.01f;
            Stat(sb, $"{flavorNames[i]} spawn", $"x{spawn:F1}", spawnMod, true);
            if (fleeMod)
                Stat(sb, $"{flavorNames[i]} flee", $"x{flee:F2}", true, false);
        }

        // ── Safety ───────────────────────────────────────────────────────────
        sb.AppendLine("<color=#66AAFF><b>Safety</b></color>");
        bool snapMod = pm.LinesLostOnSnap != 1;
        Stat(sb, "Lines/snap", pm.LinesLostOnSnap.ToString(), snapMod, false);

        statsText.text = sb.ToString();
    }

    // Single stat row — uses <pos=55> so the value column lines up regardless of label length
    static void Stat(StringBuilder sb, string label, string val, bool modified, bool higherIsBetter)
    {
        string hex = !modified ? "#888888" : (higherIsBetter ? "#55EE77" : "#FF7777");
        sb.AppendLine($"  {label}<pos=55><color={hex}><b>{val}</b></color>");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

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
}
