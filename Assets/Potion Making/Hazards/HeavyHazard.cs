using UnityEngine;

// too heavy to carry fast or throw: the player slows down and a throw just drops it
[DefaultExecutionOrder(100)] // after PlayerMovement has set this step's velocity
public class HeavyHazard : IngredientHazard
{
    // carrying speed as a fraction of walk speed
    [SerializeField, Range(0.1f, 1f)] private float carrySpeed = 0.5f;

    private PlayerMovement movement;
    private Rigidbody2D playerBody;

    protected override void OnPickedUp(GrabAndThrow by)
    {
        movement = by.GetComponent<PlayerMovement>();
        playerBody = by.GetComponent<Rigidbody2D>();
    }

    protected override void OnLetGo(GrabAndThrow by)
    {
        var v = Body.linearVelocity;
        Body.linearVelocity = new Vector2(0f, Mathf.Min(v.y, 0f));
    }

    protected override void OnFixedStep()
    {
        if (!IsHeld || movement == null || playerBody == null) return;
        // a cap rather than a multiply, so it never stacks up over frames (dash and run get capped too)
        float max = movement.walkSpeed * carrySpeed;
        var v = playerBody.linearVelocity;
        playerBody.linearVelocity = new Vector2(Mathf.Clamp(v.x, -max, max), v.y);
    }
}
