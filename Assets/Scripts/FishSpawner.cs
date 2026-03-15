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
        List<Vector3> nearby = new List<Vector3>();
        BoundsInt bounds = waterTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!waterTilemap.HasTile(pos)) continue;
            Vector3 worldPos = waterTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
            if (Vector2.Distance(worldPos, center) <= spawnRadius)
                nearby.Add(worldPos);
        }

        if (nearby.Count == 0)
        {
            Debug.LogWarning("FishSpawner: no water tiles found near cast position. Check waterTilemap assignment.");
            return;
        }

        int count = Mathf.Min(Random.Range(minSpawnCount, maxSpawnCount + 1), nearby.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, nearby.Count);
            Vector3 spawnPos = nearby[idx];
            nearby.RemoveAt(idx);

            GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
            FishInWater fw = fish.GetComponent<FishInWater>();
            if (fw != null)
            {
                fw.data = fishPool[Random.Range(0, fishPool.Length)];

                SpriteRenderer sr = fish.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Use the fish sprite as a shadow silhouette — dark and semi-transparent
                    if (fw.data.fishSprite != null)
                        sr.sprite = fw.data.fishSprite;
                    sr.color = new Color(0f, 0.05f, 0.15f, 0.45f);
                    fish.transform.localScale = Vector3.one * 0.6f;
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
}
