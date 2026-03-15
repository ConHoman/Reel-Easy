using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishingController : MonoBehaviour
{
    public Tilemap waterTilemap;
    public LineController lineController;
    public FishCaughtPopup fishPopup;

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

        if (IsWaterAt(castPos))
        {
            isFishing = true;
            movement.canMove = false;
            lineController.StartLinePhase(castPos);
        }
        else
        {
            Debug.Log("You must face water to fish.");
        }
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
        Debug.Log("Fish Escaped — line snapped!");

        if (RunManager.Instance != null)
            RunManager.Instance.LineSnapped();

        isFishing = false;
        movement.canMove = true;
    }
}
