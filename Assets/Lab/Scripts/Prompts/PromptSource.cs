using System.Collections.Generic;
using Nocturne.UI;
using UnityEngine;

public struct PromptContext
{
    public bool holding;
    public bool gamepad;
    public Vector2 position;
}

// something the player can walk up to and get a prompt for
public abstract class PromptSource : MonoBehaviour
{
    public static readonly List<PromptSource> Active = new List<PromptSource>();

    [SerializeField] protected float range = 0.4f;
    [SerializeField] private float heightRange = 2f;

    // when set, the player has to be within range of this collider's sides instead of the object's center
    protected Collider2D area;

    protected virtual void Awake() => area = GetComponent<Collider2D>();
    protected virtual void OnEnable() => Active.Add(this);
    protected virtual void OnDisable() => Active.Remove(this);

    // how far away the player is, or infinity when they're out of range
    public virtual float Distance(Vector2 player)
    {
        if (area != null)
        {
            Bounds b = area.bounds;
            float dx = Mathf.Max(0f, Mathf.Max(b.min.x - player.x, player.x - b.max.x));
            float dy = Mathf.Max(0f, Mathf.Max(b.min.y - player.y, player.y - b.max.y));
            if (dy > heightRange || dx > range) return float.PositiveInfinity;
            // being in front of or behind it counts too, so nearby things on the floor don't tie
            return dx + dy;
        }
        float d = Vector2.Distance(transform.position, player);
        return d <= range ? d : float.PositiveInfinity;
    }

    // fill in what the box should say
    public abstract bool Fill(PromptContext context, PromptContent content);

    protected static string PickKey(PromptContext context, string key, string padKey) =>
        context.gamepad && !string.IsNullOrEmpty(padKey) ? padKey : key;
}
