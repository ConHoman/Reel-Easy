using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Run these menu items in order with scene_8 open:
///   Reel Easy > 1. Create Fish Data Assets
///   Reel Easy > 2. Create Prefabs
///   Reel Easy > 3. Setup Scene
///
///   Or just: Reel Easy > Run Full Setup (does all three)
/// </summary>
public static class ReelEasySetup
{
    const string FishDataFolder = "Assets/FishData";
    const string FishIconFolder = "Assets/Sprites/Fishing Icons";

    // ─────────────────────────────────────────────────────────
    // STEP 1 — Fish ScriptableObjects
    // ─────────────────────────────────────────────────────────
    [MenuItem("Reel Easy/1. Create Fish Data Assets")]
    static void CreateFishDataAssets()
    {
        if (!AssetDatabase.IsValidFolder(FishDataFolder))
            AssetDatabase.CreateFolder("Assets", "FishData");

        // name, spriteFile, difficulty, rarity, score
        var fishDefs = new (string name, string file, int diff, int rarity, int score)[]
        {
            ("Bass",         "Bass Fish Icon.png",       1, 1,  100),
            ("Cod",          "Cod Fish Icon.png",         1, 1,  120),
            ("Salmon",       "Samon fish Icon.png",       2, 1,  200),
            ("Clown Fish",   "Clown Fish Icon.png",       2, 1,  180),
            ("Goldfish",     "Gold Fish Icon.png",        2, 2,  250),
            ("Redfin",       "Redfin Fish Icon.png",      3, 2,  350),
            ("Flatfish",     "Flat Fish Icon.png",        3, 2,  300),
            ("Fancy Fish",   "Fancy Fish Icon.png",       4, 2,  500),
            ("Iceshard Fish","Iceshard Fish Icon.png",    4, 3,  600),
            ("Magma Fish",   "Magma Fish Icon.png",       5, 3, 1000),
        };

        foreach (var def in fishDefs)
        {
            string assetPath = FishDataFolder + "/" + def.name + ".asset";
            FishData existing = AssetDatabase.LoadAssetAtPath<FishData>(assetPath);
            if (existing != null) continue;

            FishData fd = ScriptableObject.CreateInstance<FishData>();
            fd.fishName   = def.name;
            fd.difficulty = def.diff;
            fd.rarity     = def.rarity;
            fd.scoreValue = def.score;

            // Try to load the sprite
            string spritePath = FishIconFolder + "/" + def.file;
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                // Some fish icons share a sprite sheet — try loading by sub-asset
                Object[] all = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                foreach (Object o in all)
                    if (o is Sprite s) { sprite = s; break; }
            }

            if (sprite != null)
                fd.fishSprite = sprite;
            else
                Debug.LogWarning($"[Reel Easy] Could not find sprite at {spritePath} — assign manually.");

            AssetDatabase.CreateAsset(fd, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Reel Easy] Step 1 complete — FishData assets created in Assets/FishData/");
    }

    // ─────────────────────────────────────────────────────────
    // STEP 2 — Prefabs
    // ─────────────────────────────────────────────────────────
    [MenuItem("Reel Easy/2. Create Prefabs")]
    static void CreatePrefabs()
    {
        EnsureTag("LineTip");

        // LineTip prefab
        string tipPath = "Assets/Sprites/LineTip.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(tipPath) == null)
        {
            GameObject tip = new GameObject("LineTip");
            tip.tag = "LineTip";
            var col = tip.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            var sr = tip.AddComponent<SpriteRenderer>();

            // Reuse the bobber sprite as a stand-in visual
            Sprite bobberSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Bobber.prefab");
            if (bobberSprite == null)
            {
                Object[] all = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Bobber.prefab");
                foreach (Object o in all) if (o is Sprite s) { bobberSprite = s; break; }
            }
            if (bobberSprite != null) sr.sprite = bobberSprite;

            PrefabUtility.SaveAsPrefabAsset(tip, tipPath);
            Object.DestroyImmediate(tip);
            Debug.Log("[Reel Easy] Created LineTip prefab at " + tipPath);
        }

        // FishInWater prefab
        string fishPath = "Assets/Sprites/FishInWater.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(fishPath) == null)
        {
            GameObject fish = new GameObject("FishInWater");
            var col = fish.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;
            fish.AddComponent<SpriteRenderer>(); // sprite assigned per-instance by FishSpawner
            fish.AddComponent<FishInWater>();

            PrefabUtility.SaveAsPrefabAsset(fish, fishPath);
            Object.DestroyImmediate(fish);
            Debug.Log("[Reel Easy] Created FishInWater prefab at " + fishPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Reel Easy] Step 2 complete — prefabs created.");
    }

    // ─────────────────────────────────────────────────────────
    // STEP 3 — Scene setup
    // ─────────────────────────────────────────────────────────
    [MenuItem("Reel Easy/3. Setup Scene")]
    static void SetupScene()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Reel Easy] No Canvas found in scene. Add a Canvas first.");
            return;
        }

        // ── Managers ──────────────────────────────────────────
        HallOfFame hallOfFame = FindOrCreate<HallOfFame>("HallOfFame");
        RunManager runManager = FindOrCreate<RunManager>("RunManager");
        FishSpawner fishSpawner = FindOrCreate<FishSpawner>("FishSpawner");
        MinigameManager minigame = FindOrCreate<MinigameManager>("MinigameManager");

        // ── FishSpawner wiring ─────────────────────────────────
        if (fishSpawner.fishPrefab == null)
        {
            var fishPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Sprites/FishInWater.prefab");
            if (fishPrefab != null) fishSpawner.fishPrefab = fishPrefab;
        }
        if (fishSpawner.fishPool == null || fishSpawner.fishPool.Length == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:FishData", new[] { FishDataFolder });
            var pool = new List<FishData>();
            foreach (string g in guids)
                pool.Add(AssetDatabase.LoadAssetAtPath<FishData>(AssetDatabase.GUIDToAssetPath(g)));
            fishSpawner.fishPool = pool.ToArray();
        }
        if (fishSpawner.waterTilemap == null)
        {
            // Grab the water tilemap reference from FishingController
            FishingController fcForTilemap = Object.FindObjectOfType<FishingController>();
            if (fcForTilemap != null && fcForTilemap.waterTilemap != null)
            {
                fishSpawner.waterTilemap = fcForTilemap.waterTilemap;
                Debug.Log("[Reel Easy] Assigned water tilemap to FishSpawner from FishingController.");
            }
            else
                Debug.LogWarning("[Reel Easy] Could not find water tilemap — assign FishSpawner.waterTilemap manually in the Inspector.");
        }

        // ── Minigame wiring ────────────────────────────────────
        if (minigame.bubblePrefab == null)
        {
            var bubblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Sprites/Bubble.prefab");
            if (bubblePrefab != null) minigame.bubblePrefab = bubblePrefab;
        }

        // Wire fishingController early so it's always set regardless of player block below
        FishingController fcEarly = Object.FindObjectOfType<FishingController>();
        if (fcEarly != null)
            minigame.fishingController = fcEarly;

        // ── RunManager UI ──────────────────────────────────────
        if (runManager.runInfoText == null)
        {
            TMP_Text info = CreateTMPText(canvas.transform, "RunInfoText",
                anchor: new Vector2(0, 1), pivot: new Vector2(0, 1),
                pos: new Vector2(10, -10), size: new Vector2(180, 120));
            info.text = "Lines: 5\nScore: 0\nFish: 0";
            info.fontSize = 14;
            runManager.runInfoText = info;
        }

        if (runManager.runOverPanel == null)
        {
            GameObject panel = CreatePanel(canvas.transform, "RunOverPanel",
                new Color(0, 0, 0, 0.85f), Vector2.zero, Vector2.one);

            TMP_Text overText = CreateTMPText(panel.transform, "RunOverText",
                anchor: new Vector2(0.5f, 0.5f), pivot: new Vector2(0.5f, 0.5f),
                pos: Vector2.zero, size: new Vector2(400, 250));
            overText.alignment = TextAlignmentOptions.Center;
            overText.fontSize = 24;

            // Play Again button
            GameObject btnGO = new GameObject("PlayAgainButton");
            btnGO.transform.SetParent(panel.transform, false);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            var btn = btnGO.AddComponent<Button>();
            var btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = btnRT.anchorMax = btnRT.pivot = new Vector2(0.5f, 0.5f);
            btnRT.anchoredPosition = new Vector2(0, -120);
            btnRT.sizeDelta = new Vector2(160, 50);

            GameObject btnLabel = new GameObject("Label");
            btnLabel.transform.SetParent(btnGO.transform, false);
            var lbl = btnLabel.AddComponent<TextMeshProUGUI>();
            lbl.text = "Play Again";
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.fontSize = 18;
            var lblRT = btnLabel.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.sizeDelta = Vector2.zero;

            // Wire Play Again button to RunManager.StartRun
            btn.onClick.AddListener(() => runManager.StartRun());

            runManager.runOverPanel = panel;
            runManager.runOverText  = overText;

            panel.SetActive(false);
        }

        // ── Minigame UI panel ──────────────────────────────────
        if (minigame.panel == null)
        {
            // Look for an existing minigame panel first
            var existingPanel = GameObject.Find("BubblePanel") ?? GameObject.Find("MinigamePanel");
            if (existingPanel != null)
            {
                minigame.panel = existingPanel.GetComponent<RectTransform>();
            }
            else
            {
                GameObject mgPanel = CreatePanel(canvas.transform, "MinigamePanel",
                    new Color(0.1f, 0.1f, 0.3f, 0.9f),
                    new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.75f));

                TMP_Text cd = CreateTMPText(mgPanel.transform, "CountdownText",
                    anchor: new Vector2(0.5f, 1f), pivot: new Vector2(0.5f, 1f),
                    pos: new Vector2(0, -10), size: new Vector2(300, 60));
                cd.alignment = TextAlignmentOptions.Center;
                cd.fontSize = 36;

                TMP_Text info = CreateTMPText(mgPanel.transform, "HookedInfoText",
                    anchor: new Vector2(0.5f, 0f), pivot: new Vector2(0.5f, 0f),
                    pos: new Vector2(0, 10), size: new Vector2(300, 40));
                info.alignment = TextAlignmentOptions.Center;
                info.fontSize = 14;

                minigame.panel         = mgPanel.GetComponent<RectTransform>();
                minigame.countdownText = cd;
                minigame.hookedInfoText = info;

                mgPanel.SetActive(false);
            }
        }

        // ── Player / FishingController / LineController ────────
        FishingController fc = Object.FindObjectOfType<FishingController>();
        if (fc != null)
        {
            LineController lc = fc.GetComponent<LineController>();
            if (lc == null) lc = fc.gameObject.AddComponent<LineController>();

            lc.minigameManager = minigame;
            if (lc.lineTipPrefab == null)
                lc.lineTipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Sprites/LineTip.prefab");

            fc.lineController = lc;
            minigame.fishingController = fc;

            if (fc.bobberPrefab == null)
                fc.bobberPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Sprites/Bobber.prefab");

            FishInventory inv = Object.FindObjectOfType<FishInventory>();
            runManager.fishInventory = inv;
            runManager.fishSpawner   = fishSpawner;

            Debug.Log("[Reel Easy] Wired FishingController → LineController → MinigameManager");
        }
        else
        {
            Debug.LogWarning("[Reel Easy] FishingController not found — open the gameplay scene (scene_8) and run setup again.");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Reel Easy] Step 3 complete — scene saved. Check Console for any warnings about missing refs.");
    }

    // ─────────────────────────────────────────────────────────
    // CLEANUP — disable old QuestManager UI leftovers
    // ─────────────────────────────────────────────────────────
    [MenuItem("Reel Easy/Fix - Hide Old Quest UI")]
    static void HideOldQuestUI()
    {
        int fixed_ = 0;

        // Old game over panel (QuestManager left it active in the scene)
        GameObject oldPanel = GameObject.Find("GameOverPanel");
        if (oldPanel != null)
        {
            oldPanel.SetActive(false);
            Debug.Log("[Reel Easy] Disabled old GameOverPanel");
            fixed_++;
        }

        // Old quest text (the "Quest: Catch Fish" label)
        GameObject oldQuestText = GameObject.Find("QuestText");
        if (oldQuestText != null)
        {
            oldQuestText.SetActive(false);
            Debug.Log("[Reel Easy] Disabled old QuestText");
            fixed_++;
        }

        if (fixed_ == 0)
            Debug.Log("[Reel Easy] No old quest UI found — nothing to clean up.");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    // ─────────────────────────────────────────────────────────
    // RUN ALL
    // ─────────────────────────────────────────────────────────
    [MenuItem("Reel Easy/Run Full Setup")]
    static void RunAll()
    {
        CreateFishDataAssets();
        CreatePrefabs();
        SetupScene();
        HideOldQuestUI();
        Debug.Log("[Reel Easy] Full setup complete!");
    }

    // ─────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────

    static T FindOrCreate<T>(string goName) where T : Component
    {
        T existing = Object.FindObjectOfType<T>();
        if (existing != null) return existing;
        return new GameObject(goName).AddComponent<T>();
    }

    static TMP_Text CreateTMPText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return t;
    }

    static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log("[Reel Easy] Added tag: " + tag);
    }
}
