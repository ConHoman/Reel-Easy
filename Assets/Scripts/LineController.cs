using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// After casting, the player steers the line tip with WASD for a few seconds.
// Any FishInWater with a trigger collider the tip enters gets hooked.
// When time runs out, the hooked fish list is sent to MinigameManager.
//
// SETUP REQUIRED IN UNITY:
//   1. Create a LineTip prefab: small sprite + CircleCollider2D (Is Trigger) + tag "LineTip"
//   2. Assign a LineRenderer component to this GameObject (or it will be added automatically)
//   3. Assign lineTipPrefab and minigameManager in the Inspector
public class LineController : MonoBehaviour
{
    [Header("References")]
    public MinigameManager minigameManager;
    public GameObject lineTipPrefab;

    [Header("Phase Settings")]
    public float tipSpeed = 4f;
    public float phaseDuration = 3f;
    public float maxLineLength = 2.5f;

    // Static reference so FishInWater can find the active line
    public static LineController ActiveInstance;

    private GameObject lineTip;
    private LineRenderer lineRenderer;
    private List<FishData> hookedFish = new List<FishData>();
    private bool phaseActive = false;

    void Awake()
    {
        ActiveInstance = this;

        if (minigameManager == null)
            minigameManager = FindObjectOfType<MinigameManager>();

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
    }

    public void StartLinePhase(Vector2 castPos, GameObject bobberOverride = null, Tilemap waterTilemap = null)
    {
        if (lineTipPrefab == null)
        {
            Debug.LogError("LineController: lineTipPrefab is not assigned! Run Reel Easy > 3. Setup Scene.");
            return;
        }
        if (minigameManager == null)
        {
            Debug.LogError("LineController: minigameManager is not assigned! Run Reel Easy > 3. Setup Scene.");
            return;
        }
        hookedFish.Clear();
        StartCoroutine(LinePhaseRoutine(castPos, bobberOverride, waterTilemap));
    }

    IEnumerator LinePhaseRoutine(Vector2 startPos, GameObject bobberOverride = null, Tilemap waterTilemap = null)
    {
        phaseActive = true;

        // Use the bobber as the moving tip if provided, otherwise fall back to lineTipPrefab
        GameObject tipPrefab = (bobberOverride != null) ? bobberOverride : lineTipPrefab;
        lineTip = Instantiate(tipPrefab, startPos, Quaternion.identity);
        lineTip.transform.localScale = Vector3.one * 0.2f;

        // Tag must be set so FishInWater can detect it
        lineTip.tag = "LineTip";

        // Rigidbody2D required for OnTriggerEnter2D to fire
        Rigidbody2D tipRb = lineTip.GetComponent<Rigidbody2D>();
        if (tipRb == null) tipRb = lineTip.AddComponent<Rigidbody2D>();
        tipRb.isKinematic = true;
        tipRb.gravityScale = 0f;

        // Always ensure a trigger collider exists
        CircleCollider2D tipCol = lineTip.GetComponent<CircleCollider2D>();
        if (tipCol == null) tipCol = lineTip.AddComponent<CircleCollider2D>();
        tipCol.isTrigger = true;
        tipCol.radius = 0.35f;

        lineRenderer.enabled = true;

        // Tell the minigame panel to show the steering countdown
        minigameManager.BeginSteerPhase(phaseDuration);

        float timer = phaseDuration;

        while (timer > 0f && phaseActive)
        {
            float mx = Input.GetAxisRaw("Horizontal");
            float my = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(mx, my).normalized;

            Vector3 newPos = lineTip.transform.position + (Vector3)(dir * tipSpeed * Time.deltaTime);

            // Clamp to maxLineLength from cast point
            Vector2 offset = (Vector2)newPos - startPos;
            if (offset.magnitude > maxLineLength)
                newPos = (Vector2)startPos + offset.normalized * maxLineLength;

            // Only move if the new position is a water tile
            bool isWater = true;
            if (waterTilemap != null)
                isWater = waterTilemap.HasTile(waterTilemap.WorldToCell(newPos));

            if (isWater)
                lineTip.transform.position = newPos;

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, lineTip.transform.position);

            timer -= Time.deltaTime;
            yield return null;
        }

        EndPhase();
    }

    // Called by FishInWater when the tip enters its trigger
    public void HookFish(FishInWater fish)
    {
        hookedFish.Add(fish.data);
        Destroy(fish.gameObject);
    }

    void EndPhase()
    {
        phaseActive = false;

        if (lineTip != null)
        {
            Destroy(lineTip);
            lineTip = null;
        }

        lineRenderer.enabled = false;

        minigameManager.StartMinigame(new List<FishData>(hookedFish));
    }
}
