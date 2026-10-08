using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlatformDropThrough : MonoBehaviour
{
    [SerializeField] private float minIgnoreTime = 0.2f;

    private PlayerMovement playerMovement;
    private Collider2D playerCollider;
    private readonly Collider2D[] touching = new Collider2D[8];
    private readonly HashSet<Collider2D> ignoring = new HashSet<Collider2D>();

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (playerMovement.getMoveInput().y > -0.5f)
        {
            return;
        }

        int count = playerCollider.GetContacts(touching);
        for (int i = 0; i < count; i++)
        {
            Collider2D platform = touching[i];
            if (platform.GetComponent<PlatformEffector2D>() != null && IsStandingOn(platform) && !ignoring.Contains(platform))
            {
                StartCoroutine(IgnorePlatform(platform));
            }
        }
    }

    private bool IsStandingOn(Collider2D platform)
    {
        return playerCollider.bounds.min.y >= platform.bounds.max.y - 0.05f;
    }

    private IEnumerator IgnorePlatform(Collider2D platform)
    {
        ignoring.Add(platform);
        Physics2D.IgnoreCollision(playerCollider, platform, true);

        yield return new WaitForSeconds(minIgnoreTime);
        // keep ignoring until the player is fully clear, otherwise they get pushed back up onto it
        while (platform != null && playerCollider.bounds.Intersects(platform.bounds))
        {
            yield return null;
        }

        if (platform != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platform, false);
        }
        ignoring.Remove(platform);
    }
}
