using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shown every 3 successful casts. Player picks one of 3 random perks.
// Builds its own UI at runtime — no setup script needed.
public class PerkPickerUI : MonoBehaviour
{
    public static PerkPickerUI Instance;

    private GameObject pickerPanel;
    private Transform cardRow;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public static void EnsureExists()
    {
        if (Instance == null)
            new GameObject("PerkPickerUI").AddComponent<PerkPickerUI>();
    }

    public void ShowPicker()
    {
        PerkManager.EnsureExists();

        // All perks are always available — duplicates allowed for stacking.
        var available = new List<PerkDefinition>(PerkManager.AllPerks);

        if (available.Count == 0) return;

        // Fisher-Yates shuffle
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            PerkDefinition tmp = available[i];
            available[i] = available[j];
            available[j] = tmp;
        }

        int count = Mathf.Min(3, available.Count);

        BuildOrClearPanel();

        for (int i = 0; i < count; i++)
            AddCard(available[i]);

        pickerPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void BuildOrClearPanel()
    {
        if (pickerPanel != null)
        {
            foreach (Transform c in cardRow) Destroy(c.gameObject);
            return;
        }

        Canvas canvas = UICanvas.Get();

        pickerPanel = new GameObject("PerkPickerPanel");
        pickerPanel.transform.SetParent(canvas.transform, false);
        var bg = pickerPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);
        var bgRT = pickerPanel.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;

        // Title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(pickerPanel.transform, false);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "Choose a Perk";
        title.fontSize = 9;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.1f, 0.87f);
        titleRT.anchorMax = new Vector2(0.9f, 0.97f);
        titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

        // Card row
        var rowGO = new GameObject("CardRow");
        rowGO.transform.SetParent(pickerPanel.transform, false);
        var hLayout = rowGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 8f;
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childForceExpandWidth  = false;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlHeight = false;
        var rowRT = rowGO.GetComponent<RectTransform>();
        rowRT.anchorMin = new Vector2(0.02f, 0.05f);
        rowRT.anchorMax = new Vector2(0.98f, 0.84f);
        rowRT.offsetMin = rowRT.offsetMax = Vector2.zero;
        cardRow = rowGO.transform;
    }

    void AddCard(PerkDefinition def)
    {
        PerkDefinition captured = def;

        var cardGO = new GameObject("Card_" + def.displayName);
        cardGO.transform.SetParent(cardRow, false);

        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.1f, 0.13f, 0.22f, 1f);

        var btn = cardGO.AddComponent<Button>();
        btn.targetGraphic = cardImg;
        var cs = btn.colors;
        cs.highlightedColor = new Color(0.18f, 0.22f, 0.38f, 1f);
        cs.pressedColor     = new Color(0.06f, 0.08f, 0.15f, 1f);
        btn.colors = cs;

        var cardRT = cardGO.GetComponent<RectTransform>();
        cardRT.sizeDelta = new Vector2(90f, 0f); // height driven by ContentSizeFitter

        // Auto-size height to content
        var csf = cardGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout = cardGO.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(5, 5, 8, 8);
        layout.spacing = 4f;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;

        // Category badge
        MakeCardText(cardGO.transform, def.category.ToString().ToUpper(), 5f, CategoryColor(def.category), FontStyles.Bold);

        // Name
        MakeCardText(cardGO.transform, def.displayName, 8f, Color.white, FontStyles.Bold);

        // Divider
        var div = new GameObject("Divider");
        div.transform.SetParent(cardGO.transform, false);
        div.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
        div.AddComponent<LayoutElement>().preferredHeight = 2f;

        // Upside (green)
        MakeCardText(cardGO.transform, "+ " + def.upside, 6f, new Color(0.4f, 1f, 0.5f), FontStyles.Normal);

        // Downside (red) — only if there is one
        if (!string.IsNullOrEmpty(def.downside))
            MakeCardText(cardGO.transform, "- " + def.downside, 6f, new Color(1f, 0.42f, 0.42f), FontStyles.Normal);

        btn.onClick.AddListener(() => PickPerk(captured.type));
    }

    static void MakeCardText(Transform parent, string text, float size, Color color, FontStyles style)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = true;
        go.AddComponent<LayoutElement>().flexibleWidth = 1f;
    }

    static Color CategoryColor(PerkCategory cat)
    {
        switch (cat)
        {
            case PerkCategory.Safety:   return new Color(0.4f, 0.7f, 1f);    // blue
            case PerkCategory.Fishing:  return new Color(0.3f, 0.9f, 0.8f);  // teal
            case PerkCategory.Scoring:  return new Color(1f,   0.85f, 0.2f); // gold
            case PerkCategory.Minigame: return new Color(0.8f, 0.5f, 1f);    // purple
            default:                    return Color.white;
        }
    }

    void PickPerk(PerkType perk)
    {
        PerkManager.Instance.AddPerk(perk);

        if (perk == PerkType.ExtraLine && RunManager.Instance != null)
            RunManager.Instance.AddLine();
        if (perk == PerkType.SilkThread && RunManager.Instance != null)
            RunManager.Instance.RemoveLine();

        if (pickerPanel != null) pickerPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
