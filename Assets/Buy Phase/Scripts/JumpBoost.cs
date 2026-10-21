using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class JumpBoost : MonoBehaviour
{
    [SerializeField] private float pressWindow = 0.1f;
    [SerializeField] private float minSpeedGain = 2f;

    private Rigidbody2D rb;
    private InputAction jumpAction;
    private float multiplier = 1f;
    private float pressedTime = float.NegativeInfinity;
    private float lastVelocityY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // after controls set up
    private void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            jumpAction = playerInput.actions.FindAction("Jump");
        }
        OnEnable();
    }

    private void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.started -= OnJumpPressed;
            jumpAction.started += OnJumpPressed;
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.started -= OnJumpPressed;
        }
    }

    public void SetHeightMultiplier(float heightMultiplier)
    {
        multiplier = Mathf.Sqrt(heightMultiplier);
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        pressedTime = Time.time;
    }

    private void FixedUpdate()
    {
        float velocityY = rb.linearVelocity.y;

        bool jumped = Time.time - pressedTime <= pressWindow && velocityY > 0f && velocityY - lastVelocityY >= minSpeedGain;
        if (jumped)
        {
            pressedTime = float.NegativeInfinity;
            if (multiplier != 1f)
            {
                velocityY *= multiplier;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, velocityY);
            }
        }

        lastVelocityY = velocityY;
    }
}
