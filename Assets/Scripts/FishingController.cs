using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishingController : MonoBehaviour
{
    public GameObject bobberPrefab;
    public Tilemap waterTilemap;
    public BubbleGameManager bubbleGameManager;

    public float castDistance = 0.6f;
    public float minBiteTime = 1f;
    public float maxBiteTime = 2.5f;

    public GameObject activeBobber;

    private PlayerMovement movement;
    private bool isFishing = false;

    // Reference to popup object in the scene
    public FishCaughtPopup fishPopup;

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
        movement.canMove = false;

        // Create bobber object and store reference
        activeBobber = Instantiate(bobberPrefab, spawnPos, Quaternion.identity);

        float wait = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(wait);

        Debug.Log("Fish bite! Starting minigame...");

        bubbleGameManager.StartBubbleGame();
    }

    // ============================================================
    // CALLED BY THE MINIGAME 
    // ============================================================

    public void CatchFishSuccess()
    {
        Debug.Log("🎣 Fish Caught!");

        // Pick a random fish sprite
        Sprite caughtFish = null;

        if (FishInventory.Instance != null &&
            FishInventory.Instance.fishSprites.Length > 0)
        {
            int r = Random.Range(0, FishInventory.Instance.fishSprites.Length);
            caughtFish = FishInventory.Instance.fishSprites[r];
        }

        // Add fish to inventory
        if (caughtFish != null)
            FishInventory.Instance.AddFish(caughtFish);

        // 🔹 QUEST PROGRESS
        if (QuestManager.Instance != null)
            QuestManager.Instance.FishCaught();

        // Show popup above player
        if (fishPopup != null)
        {
            fishPopup.ShowMessage("Caught a " + caughtFish.name + "!");
        }

        // Remove bobber
        if (activeBobber != null)
            Destroy(activeBobber);

        isFishing = false;
        movement.canMove = true;
    }

    public void CatchFishFail()
    {
        Debug.Log("🐟 Fish Escaped...");

        // 🔹 QUEST FAIL COUNT
        if (QuestManager.Instance != null)
            QuestManager.Instance.FishFailed();

        if (activeBobber != null)
            Destroy(activeBobber);

        isFishing = false;
        movement.canMove = true;
    }
}
