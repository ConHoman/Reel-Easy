using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;   // IMPORTANT for Tilemap

public class FishingController : MonoBehaviour
{
    [Header("References")]
    public GameObject bobberPrefab;
    public Tilemap waterTilemap;   // drag your WaterTilemap here in Inspector

    [Header("Settings")]
    public float castDistance = 0.6f;
    public float minBiteTime = 1f;
    public float maxBiteTime = 2.5f;

    private PlayerMovement movement;
    private bool isFishing = false;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!isFishing && Input.GetKeyDown(KeyCode.F))
        {
            TryToFish();
        }
    }

    void TryToFish()
    {
        // which way are we facing?
        Vector2 lookDir = movement.lastMoveDir;
        if (lookDir == Vector2.zero)
            lookDir = Vector2.down;

        // world position in front of player
        Vector2 castPos = (Vector2)transform.position + lookDir * castDistance;

        if (IsWaterAt(castPos))
        {
            StartCoroutine(FishRoutine(castPos));
        }
        else
        {
            Debug.Log("You must face water to fish.");
        }
    }

    bool IsWaterAt(Vector2 worldPos)
    {
        if (waterTilemap == null)
        {
            Debug.LogWarning("No waterTilemap assigned on FishingController!");
            return false;
        }

        // convert world position to tile cell
        Vector3Int cellPos = waterTilemap.WorldToCell(worldPos);

        // true if there is a tile on the water tilemap at that cell
        return waterTilemap.HasTile(cellPos);
    }

    IEnumerator FishRoutine(Vector2 spawnPos)
    {
        isFishing = true;

        GameObject bobber = Instantiate(bobberPrefab, spawnPos, Quaternion.identity);

        float waitTime = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(waitTime);

        Debug.Log("You caught a fish!");

        Destroy(bobber);
        isFishing = false;
    }
}
