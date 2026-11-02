using UnityEngine;
using System.Reflection;

// Object will steal the item the player is currently holding
[RequireComponent(typeof(ObstacleContact))]
public class ObstacleSteal : MonoBehaviour
{
    public float holdOffsetY = 0.445f;
    public float holdOffsetX = 1.5f;

    private ObstacleContact contact;
    private Rigidbody2D heldItem;

    public Rigidbody2D HeldItem => heldItem;

    void Awake()
    { 
        contact = GetComponent<ObstacleContact>(); 
    }

    void OnEnable()
    {
        contact.Touched += OnTouched;
    }

    void OnDisable()
    {
        contact.Touched -= OnTouched;
    }

    public bool hasHeldItem()
    {
        return heldItem != null;
    }

    void OnTouched(Collider2D playerCollider)
    {
        GrabAndThrow playerGrab = playerCollider.GetComponentInParent<GrabAndThrow>();
        if (heldItem != null || !playerGrab.hasHeldItem())
        {
            return;
        }

        Rigidbody2D item = playerGrab.HeldItem;

        playerGrab.Release();

        Grab(item);
    }

    private void Grab(Rigidbody2D item)
    {
        heldItem = item;

        heldItem.linearVelocity = Vector2.zero;
        heldItem.angularVelocity = 0f;
        heldItem.simulated = false;
        heldItem.transform.SetParent(transform);
        heldItem.freezeRotation = true;

        Collider2D itemCollider = heldItem.GetComponent<Collider2D>();
        float half = itemCollider != null ? itemCollider.bounds.extents.y : 0f;
        heldItem.transform.localPosition = new Vector2(holdOffsetX, holdOffsetY + half);
    }

    public void Release()
    {
        if (heldItem == null)
        {
            return;
        }

        heldItem.transform.SetParent(null);

        heldItem.simulated = true;
        heldItem.linearVelocity = Vector2.down;

        heldItem = null;
    }
}
