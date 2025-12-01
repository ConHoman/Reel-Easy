using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    Vector2 input;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        // Correct flipping for LEFT-facing default animations
        if (input.x < 0) sr.flipX = false; // moving left = normal orientation
        if (input.x > 0) sr.flipX = true;  // moving right = flipped

        anim.SetFloat("MoveX", input.x);
        anim.SetFloat("MoveY", input.y);
        anim.SetBool("IsMoving", input.sqrMagnitude > 0.01f);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }
}
