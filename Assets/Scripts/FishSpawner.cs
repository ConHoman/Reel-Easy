using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishSpawner : MonoBehaviour
{
    public Tilemap waterTilemap;
    public GameObject fishPrefab;
    public FishData[] fishPool;
    public int spawnCount = 15;

    private List<GameObject> spawnedFish = new List<GameObject>();

    void Start()
    {
        RespawnFish();
    }

    public void RespawnFish()
    {
        // Destroy existing fish
        foreach (GameObject f in spawnedFish)
            if (f != null) Destroy(f);
        spawnedFish.Clear();

        if (waterTilemap == null || fishPrefab == null || fishPool == null || fishPool.Length == 0)
        {
            Debug.LogWarning("FishSpawner: missing references.");
            return;
        }

        // Collect all water tile world positions
        List<Vector3> waterPositions = new List<Vector3>();
        BoundsInt bounds = waterTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (waterTilemap.HasTile(pos))
                waterPositions.Add(waterTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0));
        }

        int count = Mathf.Min(spawnCount, waterPositions.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, waterPositions.Count);
            Vector3 spawnPos = waterPositions[idx];
            waterPositions.RemoveAt(idx);

            GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
            FishInWater fw = fish.GetComponent<FishInWater>();
            if (fw != null)
                fw.data = fishPool[Random.Range(0, fishPool.Length)];

            spawnedFish.Add(fish);
        }
    }
}
