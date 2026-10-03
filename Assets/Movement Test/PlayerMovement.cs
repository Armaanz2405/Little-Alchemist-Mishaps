using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveInput;

    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float jumpHoldForce = 3f;
    [SerializeField] private float maxJumpHoldTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimer = 0f;
    private bool isJumping = false;
    private float jumpHoldTimer = 0f;
    private bool isGrounded;
    
    [SerializeField] private Transform holdPoint;         
    [SerializeField] private float grabRadius = 0.6f;
    [SerializeField] private LayerMask grabbableLayer;
    private float facing = 1f;
    
    private Rigidbody2D heldItem;

    [SerializeField] private float climbSpeed = 0.2f;
    private bool isClimbing = false;
    [SerializeField] private bool canClimb= false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (isGrounded && !isJumping)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (isJumping && jumpHoldTimer < maxJumpHoldTime)
        {
            rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
            jumpHoldTimer += Time.deltaTime;
        }
        
        if (!isClimbing && canClimb && moveInput.y > 0.1f)
        {
            StartClimbing();
        }
        
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            facing = Mathf.Sign(moveInput.x);
            Vector3 p = holdPoint.localPosition;
            p.x = Mathf.Abs(p.x) * facing;
            holdPoint.localPosition = p;
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

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.linearVelocity = moveInput * climbSpeed;
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
        
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
        rb.gravityScale = 1f;
    }
    
    public void GrabInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (heldItem == null)
            {
                TryGrab();
            }
            else if (heldItem != null)
            {
                Release();
            }
        }
    }
    private void TryGrab()
    {
        Collider2D hit = Physics2D.OverlapCircle(holdPoint.position, grabRadius, grabbableLayer);

        if (hit != null && hit.attachedRigidbody != null)
        {
            Debug.Log("Grabbed: " + hit.name);
            Debug.Log("HoldPoint world pos: " + holdPoint.position + " | parent: " + (holdPoint.parent ? holdPoint.parent.name : "none"));
            heldItem = hit.attachedRigidbody;

            heldItem.bodyType = RigidbodyType2D.Kinematic;
            heldItem.linearVelocity = Vector2.zero;
            heldItem.angularVelocity = 0f;
            heldItem.simulated = false;          
            heldItem.transform.SetParent(holdPoint);
            heldItem.freezeRotation = true;
            heldItem.transform.localPosition = Vector2.zero;
        }
    }

    private void Release()
    {
        heldItem.transform.SetParent(null);

        heldItem.simulated = true;                
        heldItem.bodyType = RigidbodyType2D.Dynamic;
        heldItem.linearVelocity = rb.linearVelocity;

        heldItem = null;
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGround(collision))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGround(collision))
        {
            isGrounded = false;
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

    private bool isGrabbable(Collider2D trigger)
    {
        return trigger.gameObject.CompareTag("Grabbable");
    }
    
}
