using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishingController : MonoBehaviour
{
    public Tilemap waterTilemap;
    public LineController lineController;
    public FishCaughtPopup fishPopup;
    public GameObject bobberPrefab; // used as the moving line tip visual

    public float castDistance = 0.6f;

    private PlayerMovement movement;
    private bool isFishing = false;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!isFishing && Input.GetKeyDown(KeyCode.F))
            TryToFish();
    }

    void TryToFish()
    {
        Vector2 lookDir = movement.lastMoveDir;
        if (lookDir == Vector2.zero)
            lookDir = Vector2.down;

        Vector2 castPos = (Vector2)transform.position + lookDir * castDistance;

        if (!IsWaterAt(castPos))
        {
            Debug.Log("You must face water to fish.");
            return;
        }

        if (lineController == null)
        {
            Debug.LogError("FishingController: LineController is not assigned! Run Reel Easy > 3. Setup Scene.");
            return;
        }

        isFishing = true;
        movement.canMove = false;

        // Spawn fish near the cast point so they're always reachable
        if (FishSpawner.Instance != null)
            FishSpawner.Instance.SpawnFishNear(castPos);

        lineController.StartLinePhase(castPos, bobberPrefab, waterTilemap);
    }

    bool IsWaterAt(Vector2 worldPos)
    {
        if (waterTilemap == null) return false;
        Vector3Int cellPos = waterTilemap.WorldToCell(worldPos);
        return waterTilemap.HasTile(cellPos);
    }

    // Called when the line phase ends with no fish hooked
    public void CatchNothing()
    {
        isFishing = false;
        movement.canMove = true;
    }

    // Called by MinigameManager on success
    public void CatchFishSuccess(List<FishData> caughtFish)
    {
        Debug.Log($"[Fishing] CatchFishSuccess called. Fish: {caughtFish.Count}, Time.timeScale={Time.timeScale}");

        foreach (FishData fish in caughtFish)
        {
            if (FishInventory.Instance != null)
                FishInventory.Instance.AddFish(fish);

            if (RunManager.Instance != null)
                RunManager.Instance.OnFishCaught(fish);

            if (fishPopup != null)
                fishPopup.ShowMessage("Caught a " + fish.fishName + "!");
        }

        isFishing = false;
        movement.canMove = true;
    }

    // Called by MinigameManager on fail
    public void CatchFishFail()
    {
        Debug.Log($"[Fishing] CatchFishFail called. Time.timeScale={Time.timeScale}");

        if (RunManager.Instance != null)
            RunManager.Instance.LineSnapped();

        isFishing = false;
        movement.canMove = true;
    }
}
