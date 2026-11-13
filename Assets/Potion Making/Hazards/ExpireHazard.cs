using UnityEngine;

// shrinks over its lifetime, then is gone
public class ExpireHazard : IngredientHazard
{
    [SerializeField] private float lifetime = 20f;
    // size left the moment it expires, 0.2 = a fifth of its normal size
    [SerializeField, Range(0f, 1f)] private float endScale = 0.2f;
    // only start the clock once the player first picks it up
    [SerializeField] private bool startOnPickup = true;

    private Vector3 startScale;
    private float age;
    private bool ticking;

    protected override void Awake()
    {
        base.Awake();
        startScale = transform.lossyScale;
        ticking = !startOnPickup;
    }

    protected override void OnPickedUp(GrabAndThrow by) => ticking = true;

    private void Update()
    {
        if (!ticking || Ingredient.Used) return;
        age += Time.deltaTime;
        float t = lifetime > 0f ? Mathf.Clamp01(age / lifetime) : 1f;
        SetWorldScale(startScale * Mathf.Lerp(1f, endScale, t));
        if (t >= 1f) Expire();
    }

    // world size, so it shrinks the same while it's parented to the player
    private void SetWorldScale(Vector3 world)
    {
        var parent = transform.parent;
        if (parent == null)
        {
            transform.localScale = world;
            return;
        }
        var p = parent.lossyScale;
        transform.localScale = new Vector3(world.x / p.x, world.y / p.y, world.z / p.z);
    }

    private void Expire()
    {
        if (IsHeld) Holder.ConsumeHeldItem();
        else Destroy(gameObject);
    }
}
