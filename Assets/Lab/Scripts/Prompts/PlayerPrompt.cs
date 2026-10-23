using System;
using Nocturne.PotionMaking.Discovery;
using Nocturne.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// shows one prompt box above the player for the closest thing they can use
[RequireComponent(typeof(GrabAndThrow))]
public class PlayerPrompt : MonoBehaviour
{
    [SerializeField] private string pickUpText = "Pick up";
    [SerializeField] private string inspectText = "Inspect";
    [SerializeField] private string interactKey = "E";
    [SerializeField] private string interactPadKey = "B";
    [SerializeField] private string inspectKey = "R";
    [SerializeField] private float heightAboveHead = 0.12f;

    private GrabAndThrow grab;
    private PlayerInput playerInput;
    private SpriteRenderer body;
    private DiscoveryLog log;
    private PromptBox box;
    private IDisposable deviceWatch;
    private bool gamepad;
    private readonly PromptContent content = new PromptContent();
    private readonly PromptContent candidate = new PromptContent();

    private void Awake()
    {
        grab = GetComponent<GrabAndThrow>();
        playerInput = GetComponent<PlayerInput>();
        body = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        log = FindAnyObjectByType<DiscoveryLog>();
        box = PromptBox.Create("Player Prompt");
        deviceWatch = InputSystem.onAnyButtonPress.Call(control => gamepad = control.device is Gamepad);
    }

    private void OnDestroy()
    {
        deviceWatch?.Dispose();
        if (box != null) Destroy(box.gameObject);
    }

    private void LateUpdate()
    {
        if (box == null) return;
        if (playerInput != null && !playerInput.inputIsActive)
        {
            box.Hide();
            return;
        }

        Ingredient held = grab.HeldItem != null ? grab.HeldItem.GetComponent<Ingredient>() : null;
        var context = new PromptContext { holding = grab.HeldItem != null, held = held, gamepad = gamepad };
        Vector2 here = transform.position;
        content.Clear();
        float best = float.PositiveInfinity;

        foreach (var source in PromptSource.Active)
        {
            float d = source.Distance(here);
            if (d >= best) continue;
            candidate.Clear();
            if (!source.Fill(context, candidate)) continue;
            best = d;
            content.CopyFrom(candidate);
        }

        if (!context.holding)
        {
            Ingredient near = Grabbable();
            if (near != null && Vector2.Distance(near.transform.position, here) < best) ShowIngredient(near, context, true);
        }
        else if (content.IsEmpty && held != null)
        {
            ShowIngredient(held, context, false);
        }

        if (content.IsEmpty) box.Hide();
        else box.Show(content, Anchor());
    }

    // same check GrabAndThrow uses so it only shows when E would pick it up
    private Ingredient Grabbable()
    {
        if (grab.HoldPoint == null) return null;
        var hit = Physics2D.OverlapCircle(grab.HoldPoint.position, grab.GrabRadius, grab.GrabbableLayer);
        return hit != null && hit.attachedRigidbody != null ? hit.GetComponentInParent<Ingredient>() : null;
    }

    private void ShowIngredient(Ingredient ingredient, PromptContext context, bool canPickUp)
    {
        content.Clear();
        bool known = ingredient.data == null || log == null || log.IsDiscovered(ingredient.data);
        if (known && ingredient.data != null && canPickUp) content.header = ingredient.data.displayName;
        if (canPickUp) content.lines.Add(new PromptLine(context.gamepad ? interactPadKey : interactKey, pickUpText));
        if (!known) content.lines.Add(new PromptLine(inspectKey, inspectText));
    }

    // just above whatever is on top, the held ingredient, or the player's head
    private Vector3 Anchor()
    {
        float top = body != null ? body.bounds.max.y : transform.position.y + 1f;
        if (grab.HeldItem != null)
        {
            var heldSprite = grab.HeldItem.GetComponentInChildren<SpriteRenderer>();
            if (heldSprite != null) top = Mathf.Max(top, heldSprite.bounds.max.y);
        }
        return new Vector3(transform.position.x, top + heightAboveHead, 0f);
    }
}
