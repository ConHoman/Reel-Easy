using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishSpawner : MonoBehaviour
{
    public static FishSpawner Instance;

    public Tilemap waterTilemap;
    public GameObject fishPrefab;
    public FishData[] fishPool;

    [Tooltip("Min/max fish to spawn near the cast point each cast")]
    public int minSpawnCount = 2;
    public int maxSpawnCount = 6;
    [Tooltip("How far from the cast point fish can spawn (in world units)")]
    public float spawnRadius = 2.5f;

    private List<GameObject> spawnedFish = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        if (waterTilemap == null)
        {
            FishingController fc = FindObjectOfType<FishingController>();
            if (fc != null) waterTilemap = fc.waterTilemap;
        }

        // Always load from FishDatabase so all fish are always in the pool.
        // Once real sprites exist, apply them to FishData assets directly.
        fishPool = FishDatabase.CreateAll();
    }

    // Called at run start — no fish yet, they spawn per-cast
    public void RespawnFish()
    {
        ClearFish();
    }

    // Called by FishingController each cast — spawns fish near the cast position
    public void SpawnFishNear(Vector2 center)
    {
        ClearFish();

        if (waterTilemap == null || fishPrefab == null || fishPool == null || fishPool.Length == 0)
        {
            Debug.LogWarning("FishSpawner: missing references.");
            return;
        }

        // Collect water tiles within spawnRadius of the cast point
        float effectiveRadius = spawnRadius * (PerkManager.Instance != null ? PerkManager.Instance.SpawnRadiusMultiplier : 1f);
        List<Vector3> nearby = new List<Vector3>();
        BoundsInt bounds = waterTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!waterTilemap.HasTile(pos)) continue;
            Vector3 worldPos = waterTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
            if (Vector2.Distance(worldPos, center) <= effectiveRadius)
                nearby.Add(worldPos);
        }

        if (nearby.Count == 0)
        {
            Debug.LogWarning("FishSpawner: no water tiles found near cast position. Check waterTilemap assignment.");
            return;
        }

        // Rarity-weighted pool: common=12, uncommon=3, epic=2, legendary=1
        // Mythical (5) only enter the pool on a 4% chance per cast (boosted by Mythic Blessing)
        float mythicalChance = Mathf.Min(1f, 0.04f * (PerkManager.Instance != null ? PerkManager.Instance.MythicalGateBonus : 1f));
        bool allowMythical = Random.value < mythicalChance;
        List<FishData> weightedPool = new List<FishData>();
        foreach (FishData fd in fishPool)
        {
            if (fd.rarity == 5 && !allowMythical) continue;
            int baseWeight = fd.rarity == 5 ? 1
                           : fd.rarity == 1 ? 12
                           : fd.rarity == 2 ? 3
                           : fd.rarity == 3 ? 2
                           :                  1;  // legendary (4)
            int bonus  = PerkManager.Instance != null ? PerkManager.Instance.RaritySpawnBonus(fd.rarity) : 0;
            int weight = baseWeight + bonus;
            for (int w = 0; w < weight; w++)
                weightedPool.Add(fd);
        }

        int spawnBonus = PerkManager.Instance != null ? PerkManager.Instance.SpawnCountBonus : 0;
        int count = Mathf.Min(Random.Range(minSpawnCount, maxSpawnCount + 1) + spawnBonus, nearby.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, nearby.Count);
            Vector3 spawnPos = nearby[idx];
            nearby.RemoveAt(idx);

            GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
            FishInWater fw = fish.GetComponent<FishInWater>();
            if (fw != null)
            {
                fw.data = weightedPool[Random.Range(0, weightedPool.Count)];

                float albino  = PerkManager.Instance != null ? PerkManager.Instance.FlavorSpawnMultiplier(FishFlavor.Albino)  : 1f;
                float shiny   = PerkManager.Instance != null ? PerkManager.Instance.FlavorSpawnMultiplier(FishFlavor.Shiny)   : 1f;
                float ancient = PerkManager.Instance != null ? PerkManager.Instance.FlavorSpawnMultiplier(FishFlavor.Ancient) : 1f;
                float giant   = PerkManager.Instance != null ? PerkManager.Instance.FlavorSpawnMultiplier(FishFlavor.Giant)   : 1f;
                float golden  = PerkManager.Instance != null ? PerkManager.Instance.FlavorSpawnMultiplier(FishFlavor.Golden)  : 1f;
                float cursed  = PerkManager.Instance != null ? PerkManager.Instance.FlavorSpawnMultiplier(FishFlavor.Cursed)  : 1f;
                fw.flavor = FishFlavorData.RollWithMultipliers(albino, shiny, ancient, giant, golden, cursed);

                SpriteRenderer sr = fish.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Always assign the sprite — never fall back to the prefab's default,
                    // which would show the real fish model instead of a shadow.
                    sr.sprite = fw.data.fishSprite != null ? fw.data.fishSprite : FallbackSprite();

                    // Scale: rarity base × flavor multiplier
                    float baseScale;
                    switch (fw.data.rarity)
                    {
                        case 5: baseScale = 1.0f;  break; // mythical
                        case 4: baseScale = 0.8f;  break; // legendary
                        case 3: baseScale = 0.7f;  break; // epic
                        case 2: baseScale = 0.6f;  break; // uncommon
                        default: baseScale = 0.45f; break; // common
                    }
                    fish.transform.localScale = Vector3.one * baseScale * FishFlavorData.Get(fw.flavor).scaleMultiplier;

                    // All fish appear as dark rarity shadows in the water — flavor is hidden until caught.
                    // Size (scale) is the only in-water hint that something special may be lurking.
                    switch (fw.data.rarity)
                    {
                        case 5: sr.color = new Color(0.40f, 0f,    0.30f, 0.85f); break; // mythical
                        case 4: sr.color = new Color(0.15f, 0f,    0.25f, 0.65f); break; // legendary
                        case 3: sr.color = new Color(0.05f, 0.05f, 0.30f, 0.60f); break; // epic
                        case 2: sr.color = new Color(0f,    0.10f, 0.30f, 0.55f); break; // uncommon
                        case 1: sr.color = new Color(0f,    0.05f, 0.15f, 0.40f); break; // common
                        default: sr.color = new Color(0f,   0.05f, 0.15f, 0.40f); break;
                    }

                    sr.sortingLayerName = "Default";
                    sr.sortingOrder = 5;
                }
            }

            spawnedFish.Add(fish);
        }
    }

    void ClearFish()
    {
        foreach (GameObject f in spawnedFish)
            if (f != null) Destroy(f);
        spawnedFish.Clear();
    }

    // Returns a tiny solid-white sprite used as a shadow placeholder when no real sprite exists.
    static Sprite _fallbackSprite;
    static Sprite FallbackSprite()
    {
        if (_fallbackSprite != null) return _fallbackSprite;
        var tex = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        _fallbackSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 4f);
        return _fallbackSprite;
    }
}
