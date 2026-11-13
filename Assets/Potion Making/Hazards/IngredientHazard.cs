using UnityEngine;

// base for anything that makes an ingredient harder to handle, add any of these to an ingredient
[RequireComponent(typeof(Ingredient))]
public abstract class IngredientHazard : MonoBehaviour
{
    protected Ingredient Ingredient { get; private set; }
    protected Rigidbody2D Body { get; private set; }
    // the player carrying it right now, or null
    protected GrabAndThrow Holder { get; private set; }
    public bool IsHeld => Holder != null;

    private static GrabAndThrow player;

    protected virtual void Awake()
    {
        Ingredient = GetComponent<Ingredient>();
        Body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (player == null) player = FindAnyObjectByType<GrabAndThrow>();
        var holder = player != null && player.HeldItem == Body ? player : null;
        if (holder != Holder)
        {
            var previous = Holder;
            Holder = holder;
            if (holder != null) OnPickedUp(holder);
            else OnLetGo(previous);
        }
        OnFixedStep();
    }

    protected virtual void OnPickedUp(GrabAndThrow by) { }
    // covers both dropping and throwing
    protected virtual void OnLetGo(GrabAndThrow by) { }
    protected virtual void OnFixedStep() { }
}
