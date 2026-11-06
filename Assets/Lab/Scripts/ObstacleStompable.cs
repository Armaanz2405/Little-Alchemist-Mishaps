using UnityEngine;
using UnityEngine.Events;

// Makes an object stompable
// When player lands on it, will be stomped instead of touched
// If the object is holding an item (via ObstacleSteal for example), it will drop it
[RequireComponent(typeof(ObstacleContact))]
public class ObstacleStompable : MonoBehaviour
{
    public float jumpOnTolerance = 0.15f;
    public UnityEvent onStomped;

    private ObstacleContact contact;
    private ObstacleSteal stealItem;

    void Awake()
    {
        contact = GetComponent<ObstacleContact>();
        stealItem = GetComponent<ObstacleSteal>();
    }

    void OnEnable() 
    {
        contact.Stomped += OnStomped;
    }

    void OnDisable()
    {
        contact.Stomped -= OnStomped;
    }

    public bool IsStomp(Collider2D playerCollider, Collider2D objCollider)
    {
        return playerCollider.bounds.min.y >= objCollider.bounds.max.y - jumpOnTolerance;
    }

    void OnStomped(Collider2D player)
    {
        if (stealItem != null)
        {
            stealItem.Release();
            onStomped?.Invoke();
        }
    }
}
