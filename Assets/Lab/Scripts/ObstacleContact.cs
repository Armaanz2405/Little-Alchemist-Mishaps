using UnityEngine;

// Detects if object touches player (or if player touches object for stomping/bouncing)
// Automatically added by obstacle scripts that require collision
// Requires object to have a collider2D
[RequireComponent(typeof(Collider2D))]
public class ObstacleContact : MonoBehaviour
{
    public string playerTag = "Player";
    public float touchCooldown = 1f;

    public event System.Action<Collider2D> Touched;
    public event System.Action<Collider2D> Stomped;

    private Collider2D objCollider;
    private ObstacleStompable stompable;
    private float nextTouchAllowedTime;

    void Awake()
    {
        objCollider = GetComponent<Collider2D>();
        stompable = GetComponent<ObstacleStompable>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleContact(collision.collider);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        HandleContact(collision.collider);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleContact(other);
    }
    void OnTriggerStay2D(Collider2D other)
    {
        HandleContact(other);
    }

    void HandleContact(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }
        if (Time.time < nextTouchAllowedTime)
        {
            return;
        }

        nextTouchAllowedTime = Time.time + touchCooldown;

        bool isStomp = stompable != null && stompable.enabled && stompable.IsStomp(other, objCollider);

        if (isStomp)
        {
            Stomped?.Invoke(other);
        } else
        {
            Touched?.Invoke(other);
        }
    }
}
