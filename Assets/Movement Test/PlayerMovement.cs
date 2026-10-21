using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private float moveSpeed = 5f;
    [SerializeField]private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField]private float dashPower = 24f;
    [SerializeField]private float dashTime = 0.2f;
    [SerializeField]private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool canDash = true;
    private Vector2 moveInput;

    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float jumpHoldForce = 3f;
    [SerializeField] private float maxJumpHoldTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;
    private bool isJumping = false;
    private float jumpHoldTimer = 0f;
    private bool isGrounded;
    
    [SerializeField] private float climbSpeed = 0.2f;
    private bool isClimbing = false;
    [SerializeField] private bool canClimb= false;

    [SerializeField] private float holdDownDelay = 0.2f; 
    private float holdDownTimer;
    private bool isDropping;

    private Collider2D col;
    private readonly HashSet<Collider2D> touchingPlatforms = new HashSet<Collider2D>();
    private readonly HashSet<Collider2D> ignoredPlatforms = new HashSet<Collider2D>();
    private readonly List<Collider2D> toRestore = new List<Collider2D>();
    
    private GrabAndThrow grabAndThrow;
    private void Awake()
    { 
        
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        grabAndThrow = GetComponent<GrabAndThrow>();
        moveSpeed = walkSpeed;
    }
    private void Update()
    {
        //coyote time 
        if (isGrounded && !isJumping)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
        //climbing input
        if (!isClimbing && canClimb && moveInput.y > 0.1f && grabAndThrow.hasHeldItem() == false) 
        {
            StartClimbing();
        }
        //fall through timer
        if (moveInput.y < -0.1f){
            holdDownTimer += Time.deltaTime;
        }
        else
        {
            holdDownTimer = 0f;
        }
        
        isDropping = holdDownTimer > holdDownDelay && !isClimbing;

        if (!isDropping)
        {
            RestorePlatforms();
        }
    }
    
    private void FixedUpdate()
        {
            if (isDashing) return;  //ignore all input while dashing
            
            if (isClimbing)
            {
                //climbing movement
                rb.linearVelocity = moveInput * climbSpeed;
            }
            else
            {
                //ground movement
                rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            }
            
            if (isJumping && jumpHoldTimer < maxJumpHoldTime)
            {
                rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
                jumpHoldTimer += Time.deltaTime;
            }
    
            if (isDropping)
            {
                FallThrough();
            }
        }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.started && coyoteTimer > 0f)
        {
            Jump();
        }
        if (isClimbing)
        {
            Jump();
            StopClimbing();
        }

        if (context.canceled)
        {
            isJumping = false;
        }
    }
    
    
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        jumpHoldTimer = 0f;
        coyoteTimer = 0f;
    }

    public void Movement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void StartClimbing()
    {
        isClimbing = true;
        isJumping = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = 4f;
    }


    private void FallThrough()
    {

        foreach (var platform in touchingPlatforms)
        {
            IgnorePlatform(platform);
        }
        touchingPlatforms.Clear();
    }
    private void IgnorePlatform(Collider2D platform)
    {
        Physics2D.IgnoreCollision(col, platform, true);
        ignoredPlatforms.Add(platform);
    }

    private void RestorePlatforms()
    {
        if (ignoredPlatforms.Count == 0)
        {
            return;
        }

        toRestore.Clear();
        
        foreach (var platform in ignoredPlatforms)
        {
            if (platform == null || !col.Distance(platform).isOverlapped)
                toRestore.Add(platform);
        }

        foreach (var platform in toRestore)
        {
            if (platform != null) Physics2D.IgnoreCollision(col, platform, false);
            ignoredPlatforms.Remove(platform);
        }
    }

    private bool IsDropPlatform(Collider2D other)
    {
        return other.usedByEffector && other.GetComponent<PlatformEffector2D>() != null;
    }

    public void DashInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (canDash) StartCoroutine(Dash());
            moveSpeed = runSpeed;
        }
        if (context.canceled)
        {
            Debug.Log("Stop run");
            moveSpeed = walkSpeed;
        }
    }

    private IEnumerator Dash()
    {
        Debug.Log("Dash Started");
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale; 
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(grabAndThrow.getFacing() * dashPower, 0f);
        
        yield return new WaitForSeconds(dashTime);
        
        isDashing = false;
        rb.gravityScale = originalGravity;
       
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    private void OnTriggerEnter2D(Collider2D trigger)
    {
        if (IsClimbable(trigger))
        {
            canClimb = true;
        }
    }
    private void OnTriggerExit2D(Collider2D trigger)
    {
        if (IsClimbable(trigger))
        {
            canClimb = false;
            StopClimbing();
        }
    }
    private bool IsGround(Collision2D collision)
    {
        return collision.gameObject.CompareTag("Ground");
    }
    private bool IsClimbable(Collider2D trigger)
    {
        return trigger.gameObject.CompareTag("Climbable");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGround(collision)) isGrounded = true;

        if (IsDropPlatform(collision.collider))
        {
            if (isDropping) {
                IgnorePlatform(collision.collider);
            }
            else
            {
                touchingPlatforms.Add(collision.collider);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGround(collision)) isGrounded = false;
        touchingPlatforms.Remove(collision.collider);
    }
    
    public Vector2 getMoveInput()
    {
        return moveInput;
    }
}
