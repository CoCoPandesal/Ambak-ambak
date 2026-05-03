using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TryMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 10f;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public LayerMask groundLayer;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private int wallDirection; // -1 for left, 1 for right
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead) return;

        HandleInput();
        CheckGround();
        CheckWall();
        HandleWallSlide();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        HandleMovement();
    }

    void HandleInput()
    {
        // Left / Right
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Jump (Up)
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isTouchingWall)
            {
                WallJump();
            }
        }

        // Action Down (Fast Fall)
        if (Input.GetAxisRaw("Vertical") < 0 && !isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, -5f);
        }
    }

    void HandleMovement()
    {
        // Apply horizontal force for momentum feel
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        // FIX: Added null check to prevent NullReferenceException if Animator is not assigned
        if (animator != null) animator.SetTrigger("Jump");
    }

    void WallJump()
    {
        // Jump away from the wall
        Vector2 jumpDirection = new Vector2(-wallDirection, 1f);
        rb.velocity = new Vector2(jumpDirection.x * wallJumpForce, jumpForce * 0.8f);

        // FIX: Added null check to prevent NullReferenceException if Animator is not assigned
        if (animator != null) animator.SetTrigger("Jump");
        isWallSliding = false;
    }

    void CheckGround()
    {
        // Check if player is on a floor
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    void CheckWall()
    {
        // Check Left Wall
        if (Physics2D.OverlapCircle(wallCheckLeft.position, 0.2f, groundLayer))
        {
            isTouchingWall = true;
            wallDirection = -1;
        }
        // Check Right Wall
        else if (Physics2D.OverlapCircle(wallCheckRight.position, 0.2f, groundLayer))
        {
            isTouchingWall = true;
            wallDirection = 1;
        }
        else
        {
            isTouchingWall = false;
            wallDirection = 0;
        }
    }

    void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && rb.velocity.y < 0)
        {
            isWallSliding = true;
            // Slow down fall when sliding on wall
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    void UpdateAnimations()
    {
        // FIX: Added null check to prevent NullReferenceException if Animator is not assigned
        if (animator != null)
        {
            // Pass speed to animator for movement animation
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("isGrounded", isGrounded);
            animator.SetBool("isWallSliding", isWallSliding);
        }
    }

    // Call this from LavaManager when player dies
    public void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        // Trigger Death Animation here if you have one
        Debug.Log("Game Over!");
        // FIX: Added missing GameOver() call — without this, the game never restarts
        //GameManager.Instance.GameOver();
    }
}
