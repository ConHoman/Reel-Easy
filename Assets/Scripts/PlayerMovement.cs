using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 3f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    Vector2 input;

    // Remember which way the player is facing
    public Vector2 lastMoveDir = Vector2.down;

    // NEW: allows fishing to disable movement
    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Only allow movement if canMove is true
        if (canMove)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            input = Vector2.zero; // freeze movement
        }

        bool isMoving = input.sqrMagnitude > 0.01f;

        // Update facing direction when moving
        if (isMoving)
        {
            lastMoveDir = input.normalized;
        }

        // Sprite flipping
        if (input.x < 0) sr.flipX = false;
        if (input.x > 0) sr.flipX = true;

        // Animator parameters
        anim.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            anim.SetFloat("MoveX", input.x);
            anim.SetFloat("MoveY", input.y);
        }
        else
        {
            anim.SetFloat("MoveX", lastMoveDir.x);
            anim.SetFloat("MoveY", lastMoveDir.y);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }
}
