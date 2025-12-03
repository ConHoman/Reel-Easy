using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishingController : MonoBehaviour
{
    public GameObject bobberPrefab;
    public Tilemap waterTilemap;

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
        Vector2 lookDir = movement.lastMoveDir;
        if (lookDir == Vector2.zero)
            lookDir = Vector2.down;

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
            Debug.LogWarning("No water tilemap assigned!");
            return false;
        }

        Vector3Int cellPos = waterTilemap.WorldToCell(worldPos);
        return waterTilemap.HasTile(cellPos);
    }

    IEnumerator FishRoutine(Vector2 spawnPos)
    {
        isFishing = true;

        // NEW: stop movement while fishing
        movement.canMove = false;

        GameObject bobber = Instantiate(bobberPrefab, spawnPos, Quaternion.identity);

        float wait = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(wait);

        Debug.Log("You caught a fish!");

        Destroy(bobber);

        // NEW: re-enable movement
        movement.canMove = true;

        isFishing = false;
    }
}
