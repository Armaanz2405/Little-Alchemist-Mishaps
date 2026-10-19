using UnityEngine;

// Put this on a grabbable object (Grabbable layer + Rigidbody2D, so PlayerMovement can pick it up)
// to tell the cauldron which ingredient it is.
[RequireComponent(typeof(Rigidbody2D))]
public class Ingredient : MonoBehaviour
{
    public IngredientData data;

    // Set once a cauldron takes it, so overlapping triggers can't count it twice.
    public bool Used { get; private set; }

    private void Awake() => ApplyLook();
    private void OnValidate() => ApplyLook();

    public void ApplyLook()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || data == null) return;
        if (data.sprite != null) sr.sprite = data.sprite;
        sr.color = data.color;
    }

    public bool TryUse()
    {
        if (Used) return false;
        Used = true;
        return true;
    }
}
