using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerWalkAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] walkFrames = new Sprite[2];
    [SerializeField] private Sprite[] holdingFrames = new Sprite[2];
    [SerializeField] private float framesPerSecond = 6f;
    [SerializeField] private float minWalkSpeed = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private GrabAndThrow grabAndThrow;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        grabAndThrow = GetComponent<GrabAndThrow>();
    }

    private void LateUpdate()
    {
        bool holding = grabAndThrow != null && grabAndThrow.hasHeldItem();
        Sprite[] frames = holding && HasFrames(holdingFrames) ? holdingFrames : walkFrames;
        if (!HasFrames(frames))
        {
            return;
        }

        int frame = 0;
        if (IsWalking())
        {
            timer += Time.deltaTime;
            frame = (int)(timer * framesPerSecond) % FilledCount(frames);
        }
        else
        {
            timer = 0f;
        }
        spriteRenderer.sprite = frames[frame];
    }

    private bool IsWalking()
    {
        if (rb == null)
        {
            return false;
        }
        Vector2 velocity = rb.linearVelocity;
        return Mathf.Abs(velocity.x) > minWalkSpeed && Mathf.Abs(velocity.y) < minWalkSpeed;
    }

    private static bool HasFrames(Sprite[] frames)
    {
        return frames != null && frames.Length > 0 && frames[0] != null;
    }

    private static int FilledCount(Sprite[] frames)
    {
        int count = 0;
        while (count < frames.Length && frames[count] != null)
        {
            count++;
        }
        return count;
    }
}
