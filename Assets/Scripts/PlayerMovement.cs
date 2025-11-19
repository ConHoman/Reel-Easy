using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get WASD/Arrow input
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        input = input.normalized;
    }

    void FixedUpdate()
    {
        // Smooth movement
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}