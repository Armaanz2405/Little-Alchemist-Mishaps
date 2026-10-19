using UnityEngine;

// A brewed potion. It's grabbable too (Grabbable layer + Rigidbody2D) so it can be carried to a customer.
[RequireComponent(typeof(Rigidbody2D))]
public class Potion : MonoBehaviour
{
    public PotionData data;

    private void Awake() => ApplyLook();
    private void OnValidate() => ApplyLook();

    public void ApplyLook()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || data == null) return;
        if (data.sprite != null) sr.sprite = data.sprite;
        sr.color = data.color;
    }
}
