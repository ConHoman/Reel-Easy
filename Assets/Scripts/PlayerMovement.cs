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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Raw movement input
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        bool isMoving = input.sqrMagnitude > 0.01f;

        // Update facing direction only while moving
        if (isMoving)
        {
            lastMoveDir = input.normalized;
        }

        // Sprite flipping (based on movement X)
        if (input.x < 0) sr.flipX = false;
        if (input.x > 0) sr.flipX = true;

        // Animator parameters
        anim.SetBool("IsMoving", isMoving);

        if (isMoving)
        {
            // Use input while moving
            anim.SetFloat("MoveX", input.x);
            anim.SetFloat("MoveY", input.y);
        }
        else
        {
            // Use lastMoveDir while standing still
            anim.SetFloat("MoveX", lastMoveDir.x);
            anim.SetFloat("MoveY", lastMoveDir.y);
        }
    }

    void FixedUpdate()
    {
        // Movement
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }
}
