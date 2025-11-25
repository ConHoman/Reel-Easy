using UnityEngine;

public class PlayerIdleController : MonoBehaviour
{
    public Animator animator;           // Drag your Animator here
    public SpriteRenderer spriteRenderer; // Drag your SpriteRenderer here

    // Helper to ensure only one idle bool is true
    private void SetIdleState(bool down, bool downSide, bool side, bool upSide, bool up)
    {
        animator.SetBool("IdleDown", down);
        animator.SetBool("IdleDownSide", downSide);
        animator.SetBool("IdleSide", side);
        animator.SetBool("IdleUpSide", upSide);
        animator.SetBool("IdleUp", up);
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v);

        // ---- No input ----
        if (move == Vector2.zero)
        {
            // Keep last idle direction (optional: default to Down)
            return;
        }

        // ---- LEFT / RIGHT FLIP (default sprite faces LEFT) ----
        if (h > 0) spriteRenderer.flipX = true;   // Moving right
        else if (h < 0) spriteRenderer.flipX = false; // Moving left

        // ---- DIAGONALS ----
        if (move.x != 0 && move.y != 0)
        {
            if (move.y > 0)
                SetIdleState(false, false, false, true, false); // UpSide
            else
                SetIdleState(false, true, false, false, false); // DownSide
            return;
        }

        // ---- HORIZONTAL ONLY ----
        if (move.x != 0 && move.y == 0)
        {
            SetIdleState(false, false, true, false, false); // Side
            return;
        }

        // ---- VERTICAL ONLY ----
        if (move.y > 0)
            SetIdleState(false, false, false, false, true); // Up
        else
            SetIdleState(true, false, false, false, false); // Down
    }
}
