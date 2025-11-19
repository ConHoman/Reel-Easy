using UnityEngine;

public class PlayerIdle : MonoBehaviour
{
    public Animator animator;

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical);

        // Determine Direction based on input
        int direction;

        if (movement != Vector2.zero)
        {
            // Input detected → decide dominant direction
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                direction = 2; // Side
                // Flip sprite for left/right
                transform.localScale = movement.x > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
            }
            else
            {
                direction = movement.y > 0 ? 1 : 0; // Up or Down
                transform.localScale = new Vector3(1, 1, 1); // Reset scale for vertical
            }
        }
        else
        {
            // No input → default to down
            direction = 0;
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Update Animator
        animator.SetInteger("Direction", direction);
    }
}