using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private GrabAndThrow grabAndThrow;
    
    [Header("Walking")]
    private float moveSpeed = 5f;
    public float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    
    [Header("Dashing")]
    [SerializeField]private float dashPower = 24f;
    [SerializeField]private float dashTime = 0.2f;
    [SerializeField]private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool canDash = true;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float jumpHoldForce = 3f;
    [SerializeField] private float maxJumpHoldTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;
    private bool isJumping = false;
    private float jumpHoldTimer = 0f;
    private readonly HashSet<Collider2D> groundContacts = new HashSet<Collider2D>();
    private bool isGrounded => groundContacts.Count > 0;
    
    [Header("Double Jump")]
    [SerializeField] private float doubleJumpForce = 1f;
    [SerializeField] private float doubleJumpHoldForce = 3f;
    [SerializeField] private float maxDoubleJumpHoldTime = 0.2f;
    [SerializeField] private bool doubleJumpUnlocked = false;
    [SerializeField] private bool canDoubleJump = false;
    private bool isDoubleJumping = false;
    private float doubleJumpHoldTimer = 0f;

    [Header("Float")]
    [SerializeField] private float floatspeed = 4f;
    [SerializeField] private float floatFallspeed = 0.2f;
    [SerializeField] private bool floatUnlocked = false;
    private bool isFloating = false;
    
   
    
    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 0.2f;
    private bool isClimbing = false;
    [SerializeField] private bool canClimb= false;
    [SerializeField] private LayerMask climbableLayer;
    private WeakLadder currentWeakLadder;
    private WeakLadder currentClimbedLadder; 

    [Header("FallThrough")]
    [SerializeField] private float holdDownDelay = 0.2f; 
    private float holdDownTimer;
    private bool isDropping;
    
    public bool isPushing = false;

    private Collider2D col;
    private readonly HashSet<Collider2D> touchingPlatforms = new HashSet<Collider2D>();
    private readonly HashSet<Collider2D> ignoredPlatforms = new HashSet<Collider2D>();
    private readonly List<Collider2D> toRestore = new List<Collider2D>();

    //  up and down walk deeper or closer instead of falling and jump lands back at the same depth
    [Header("Floorboards")]
    [SerializeField] private bool walkOnFloorboards = false;
    [SerializeField] private float floorBackY = -2.33f;
    [SerializeField] private float floorFrontY = -4.1f;
    [SerializeField] private float depthSpeed = 4.5f;
    private bool onFloorboards;
    private bool floorJumping;
    private float floorJumpTime;
    private float floorY;
    private float normalGravity = 4f;

    public bool OnFloorboards => onFloorboards;
    public bool FloorJumping => floorJumping;
    public float FloorY => floorY;

    private void Awake()
    { 
        
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        grabAndThrow = GetComponent<GrabAndThrow>();
        moveSpeed = walkSpeed;
        if (rb.gravityScale > 0f) normalGravity = rb.gravityScale;
    }
    private void Update()
    {
        //coyote time 
        if (isGrounded && rb.linearVelocity.y <= 0.01f && !isJumping)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;
        //climbing input
        
        CheckClimbable(); 
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
            if (isPushing) return; //ignnore all input while pushing
            UpdateFloorboards();
            if (onFloorboards)
            {
                FloorboardMovement();
                return;
            }

            if (isClimbing)
            {
                //climbing movement
                rb.linearVelocity = moveInput * climbSpeed;
                // climbing down a ladder puts you back on the floorboards
                if (walkOnFloorboards && moveInput.y < 0f && col.bounds.min.y <= floorBackY) StopClimbing();
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

            if (isDoubleJumping && doubleJumpHoldTimer < maxDoubleJumpHoldTime)
            {
                rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Force);
                doubleJumpHoldTimer += Time.deltaTime;
            }
    
            if (isDropping)
            {
                FallThrough();
            }
        }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (onFloorboards)
        {
            if (context.started && !floorJumping)
            {
                floorJumping = true;
                floorJumpTime = Time.time;
                rb.gravityScale = normalGravity;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                Jump();
            }    
            if (context.canceled) isJumping = false;
        }
        
        if (context.started && isClimbing)
        {
            Debug.Log("Jump form ladder");
            Jump();
            StopClimbing();
        }
        else if (context.started && coyoteTimer > 0f)
        {
            Debug.Log("Jump coyote");
            Jump();
        }
        
        if (context.started && canDoubleJump && !isJumping)
        {
            Debug.Log("Double Jump");
            DoubleJump();
        }

        if (!isGrounded && floatUnlocked)
        {
            if (context.started && !isJumping && !isDoubleJumping)
            {
                Debug.Log("Float start after DJ");
                startFloat();
            }
            else if (context.started && !isJumping && !doubleJumpUnlocked)
            {
                Debug.Log("Float start no DJ");
                startFloat();
            }
        }

        if (context.canceled && isFloating)
        {
            Debug.Log("Float end");
            endFloat();
        }

        if (context.canceled && isJumping)
        {
            isJumping = false;
        }

        if (context.canceled && isDoubleJumping)
        {
            isDoubleJumping = false;
        }
        
    }

    private void Jump()
    {
        if (isJumping) return;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpHoldTimer = 0f;
        coyoteTimer = 0f;
        if (doubleJumpUnlocked)
        {
            canDoubleJump = true;
        }
        isJumping = true;
        groundContacts.Clear();
    }
    private void DoubleJump()
    {
       if (isDoubleJumping) return;
       rb.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
       isDoubleJumping = true;
       canDoubleJump = false;
       doubleJumpHoldTimer = 0f;
    }

    private void startFloat()
    {
        if (isFloating) return;
        rb.linearVelocityY = 0f;
        rb.gravityScale = floatFallspeed;
        moveSpeed = floatspeed;
        isFloating = true;
    }

    private void endFloat()
    {
        rb.gravityScale = normalGravity;
        isFloating = false;
        moveSpeed = walkSpeed;
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
        
        currentClimbedLadder = currentWeakLadder;
        if (currentWeakLadder != null)
        {
            currentWeakLadder.BeingClimbed();
            currentWeakLadder.LossDurabilty();
        }
    }

    private void StopClimbing()
    {
        if (currentClimbedLadder != null)
        {
            currentClimbedLadder.StopClimbed();
            currentClimbedLadder = null;
        }
        isClimbing = false;
        rb.gravityScale = 4f;
    }
    
    private void CheckClimbable()
    {
        Collider2D hit = Physics2D.OverlapBox(col.bounds.center, col.bounds.size, 0f, climbableLayer);

        canClimb = hit != null;
        Debug.Log("Climbable: " + canClimb);
        if (canClimb && hit.TryGetComponent(out WeakLadder ladder))
        {
            currentWeakLadder = ladder;   
        }
        else currentWeakLadder = null;
        
        if (!canClimb && isClimbing)
        {
            StopClimbing();
        }
    }

    // landing on or walking onto the floorboards from a shelf or ladder
    private void UpdateFloorboards()
    {
        if (!walkOnFloorboards || isClimbing)
        {
            onFloorboards = false;
            return;
        }
        if (!onFloorboards && col.bounds.min.y <= floorBackY + 0.02f && rb.linearVelocity.y <= 0f)
        {
            onFloorboards = true;
            floorJumping = false;
            floorY = Mathf.Clamp(col.bounds.min.y, floorFrontY, floorBackY);
            isJumping = false;
        }
    }

    private void FloorboardMovement()
    {
        float feet = col.bounds.min.y;
        if (floorJumping)
        {
            // landed on a shelf mid jump
            if (isGrounded && feet > floorBackY + 0.05f)
            {
                onFloorboards = false;
                floorJumping = false;
                return;
            }
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            if (isJumping && jumpHoldTimer < maxJumpHoldTime)
            {
                rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
                jumpHoldTimer += Time.deltaTime;
            }
            // land once falling back to the starting depth, or on the front edge of the floor
            bool rising = Time.time - floorJumpTime < 0.1f || rb.linearVelocity.y > 0.01f;
            if (rising || (feet > floorY + 0.02f && !isGrounded)) return;
            floorJumping = false;
            isJumping = false;
        }
        rb.gravityScale = 0f;
        floorY = Mathf.Clamp(floorY + moveInput.y * depthSpeed * Time.fixedDeltaTime, floorFrontY, floorBackY);
        float targetY = floorY + (rb.position.y - feet);
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, (targetY - rb.position.y) / Time.fixedDeltaTime);
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
        if (isPushing) return; //don't dash while pushing a ladder
        
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

    private bool IsGround(Collision2D collision)
    {
        return collision.gameObject.CompareTag("Ground");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateGroundContact(collision);

        if (IsDropPlatform(collision.collider))
        {
            if (isDropping) IgnorePlatform(collision.collider);
            else touchingPlatforms.Add(collision.collider);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateGroundContact(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundContacts.Remove(collision.collider);
        touchingPlatforms.Remove(collision.collider);
    }

    private void UpdateGroundContact(Collision2D collision)
    {
        if (!IsGround(collision)) return;

        // only grounded if our feet are at or above the top of what we touch
        bool feetAbove = col.bounds.min.y >= collision.collider.bounds.max.y - 0.05f;
        if (feetAbove) groundContacts.Add(collision.collider);
        else groundContacts.Remove(collision.collider);
    }
    
    public Vector2 getMoveInput()
    {
        return moveInput;
    }
    public void SetCurrentSpeed(float speed)
    {
        moveSpeed = speed;
    }
}
