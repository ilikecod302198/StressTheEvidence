using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Vector2 lastMovement = Vector2.down; // default facing down

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Input (WASD)
        movement.x = Keyboard.current.dKey.isPressed ? 1f :
                     Keyboard.current.aKey.isPressed ? -1f : 0f;

        movement.y = Keyboard.current.wKey.isPressed ? 1f :
                     Keyboard.current.sKey.isPressed ? -1f : 0f;

        movement = movement.normalized;

        bool isMoving = movement != Vector2.zero;

        // Store last direction (for idle facing)
        if (isMoving)
        {
            lastMovement = movement;
        }

        // Flip sprite for left/right
        if (movement.x > 0)
            spriteRenderer.flipX = false;
        else if (movement.x < 0)
            spriteRenderer.flipX = true;
        else if (!isMoving && lastMovement.x != 0)
            spriteRenderer.flipX = lastMovement.x < 0;

        // Set animation state
        animator.SetInteger("AnimState", GetAnimationState(isMoving, movement, lastMovement));
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    int GetAnimationState(bool isMoving, Vector2 move, Vector2 lastMove)
    {
        Vector2 dir = isMoving ? move : lastMove;

        bool horizontalDominant = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        if (isMoving)
        {
            if (horizontalDominant)
                return 5; // WalkSide
            else if (dir.y > 0)
                return 4; // WalkUp
            else
                return 3; // WalkDown
        }
        else
        {
            if (horizontalDominant)
                return 2; // IdleSide
            else if (dir.y > 0)
                return 1; // IdleUp
            else
                return 0; // IdleDown
        }
    }
}