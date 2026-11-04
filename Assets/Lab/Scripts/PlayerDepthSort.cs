using UnityEngine;

// draws the player in the right order for where they are
// on the floorboards nearer means in front, on the shelves they go behind the pillars between shelf arches
[RequireComponent(typeof(PlayerMovement))]
public class PlayerDepthSort : MonoBehaviour
{
    [SerializeField] private float floorBackY = -2.33f;
    [SerializeField] private float middleArchLeft = -2.56f;
    [SerializeField] private float middleArchRight = 2.56f;
    [SerializeField] private int shelfOrder = -32;

    private PlayerMovement movement;
    private GrabAndThrow grab;
    private SpriteRenderer body;
    private Collider2D col;
    private SpriteRenderer held;
    private int heldOriginalOrder;

    // floor objects use this too so the player and props sort by how far forward they stand
    public static int FloorOrder(float feetY) => 100 - Mathf.RoundToInt(feetY * 20f);

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        grab = GetComponent<GrabAndThrow>();
        body = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        float x = transform.position.x;
        float feet = movement.OnFloorboards ? movement.FloorY : col.bounds.min.y;
        bool onShelves = !movement.OnFloorboards && feet > floorBackY + 0.05f && (x < middleArchLeft || x > middleArchRight);
        int order = onShelves ? shelfOrder : FloorOrder(Mathf.Min(feet, floorBackY));
        body.sortingOrder = order;

        // the held ingredient sits just in front of the player, and goes back to normal once let go
        var current = grab != null && grab.HeldItem != null ? grab.HeldItem.GetComponentInChildren<SpriteRenderer>() : null;
        if (current != held)
        {
            if (held != null) held.sortingOrder = heldOriginalOrder;
            held = current;
            if (held != null) heldOriginalOrder = held.sortingOrder;
        }
        if (held != null) held.sortingOrder = order + 1;
    }
}
