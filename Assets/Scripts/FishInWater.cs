using UnityEngine;

// Attach this to fish prefabs that swim in the water.
// Give the prefab a CircleCollider2D set to "Is Trigger".
// Tag the LineController's tip object with "LineTip".
public class FishInWater : MonoBehaviour
{
    public FishData data;

    // Simple idle bob animation
    float bobSpeed;
    float bobAmount;
    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        bobSpeed = Random.Range(1f, 2.5f);
        bobAmount = Random.Range(0.05f, 0.15f);
    }

    void Update()
    {
        transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * bobSpeed) * bobAmount, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("LineTip")) return;

        LineController lc = other.GetComponentInParent<LineController>();
        if (lc == null) lc = other.GetComponent<LineController>();

        if (lc != null)
            lc.HookFish(this);
    }
}
