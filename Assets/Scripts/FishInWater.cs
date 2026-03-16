using UnityEngine;
using UnityEngine.Tilemaps;

// Attach this to fish prefabs that swim in the water.
// Fish flee from the bobber tip; rarer fish detect it sooner and flee faster.
public class FishInWater : MonoBehaviour
{
    public FishData   data;
    public FishFlavor flavor = FishFlavor.None;

    float bobSpeed;
    float bobAmount;
    Vector3 startPos;
    Vector3 spawnPos;

    float detectionRadius;
    float fleeSpeed;
    const float EscapeDistance = 5f;

    Tilemap waterTilemap;
    SpriteRenderer sr;

    void Start()
    {
        spawnPos = transform.position;
        startPos = transform.position;
        bobSpeed  = Random.Range(1f, 2.5f);
        bobAmount = Random.Range(0.05f, 0.15f);

        sr = GetComponent<SpriteRenderer>();

        var fc = FindObjectOfType<FishingController>();
        if (fc != null) waterTilemap = fc.waterTilemap;

        int rarity = data != null ? data.rarity : 1;
        switch (rarity)
        {
            case 5: detectionRadius = 3.5f; fleeSpeed = 5.0f; break; // mythical
            case 4: detectionRadius = 2.8f; fleeSpeed = 4.0f; break; // legendary
            case 3: detectionRadius = 2.1f; fleeSpeed = 2.5f; break; // epic
            case 2: detectionRadius = 1.8f; fleeSpeed = 1.8f; break; // uncommon
            default: detectionRadius = 1.1f; fleeSpeed = 0.8f; break; // common
        }

        // Flavors boost detection radius and flee speed based on their difficulty delta
        int flavorDelta = FishFlavorData.Get(flavor).difficultyDelta;
        if (flavorDelta > 0)
        {
            detectionRadius *= 1f + flavorDelta * 0.2f;
            fleeSpeed       *= 1f + flavorDelta * 0.35f;
        }
    }

    void Update()
    {
        Vector2? tip = LineController.TipPosition;

        if (tip.HasValue)
        {
            float detMult   = PerkManager.Instance != null ? PerkManager.Instance.FishDetectionMultiplier : 1f;
            float speedMult = PerkManager.Instance != null ? PerkManager.Instance.FishFleeSpeedMultiplier : 1f;
            if (PerkManager.Instance != null && data != null)
            {
                speedMult *= PerkManager.Instance.RarityFleeMultiplier(data.rarity);
                speedMult *= PerkManager.Instance.FlavorFleeMultiplier(flavor);
            }

            float dist = Vector2.Distance(startPos, tip.Value);
            if (dist < detectionRadius * detMult)
            {
                Vector2 fleeDir = ((Vector2)startPos - tip.Value).normalized;
                Vector3 newPos = startPos + (Vector3)(fleeDir * fleeSpeed * speedMult * Time.deltaTime);

                // Only move if the target tile is water
                if (IsWater(newPos))
                {
                    // Flip sprite to face flee direction
                    if (sr != null && Mathf.Abs(fleeDir.x) > 0.1f)
                        sr.flipX = fleeDir.x < 0;

                    startPos = newPos;
                }

                if (Vector2.Distance(startPos, spawnPos) > EscapeDistance)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * bobSpeed) * bobAmount, 0);
    }

    bool IsWater(Vector3 worldPos)
    {
        if (waterTilemap == null) return true; // no tilemap = don't block movement
        return waterTilemap.HasTile(waterTilemap.WorldToCell(worldPos));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("LineTip")) return;
        if (LineController.ActiveInstance != null)
            LineController.ActiveInstance.HookFish(this);
    }
}
