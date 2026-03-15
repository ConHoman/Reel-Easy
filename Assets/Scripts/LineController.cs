using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private GameObject lineTip;
    private LineRenderer lineRenderer;
    private List<FishData> hookedFish = new List<FishData>();
    private bool phaseActive = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
    }

    public void StartLinePhase(Vector2 castPos)
    {
        hookedFish.Clear();
        StartCoroutine(LinePhaseRoutine(castPos));
    }

    IEnumerator LinePhaseRoutine(Vector2 startPos)
    {
        phaseActive = true;

        lineTip = Instantiate(lineTipPrefab, startPos, Quaternion.identity);
        lineRenderer.enabled = true;

        float timer = phaseDuration;

        while (timer > 0f && phaseActive)
        {
            float mx = Input.GetAxisRaw("Horizontal");
            float my = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(mx, my).normalized;

            lineTip.transform.position += (Vector3)(dir * tipSpeed * Time.deltaTime);

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
