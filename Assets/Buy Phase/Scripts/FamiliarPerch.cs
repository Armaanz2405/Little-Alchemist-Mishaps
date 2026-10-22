using System.Collections.Generic;
using UnityEngine;

public class FamiliarPerch : MonoBehaviour
{
    [SerializeField] private JumpBoost playerJumpBoost;
    [SerializeField] private float spacing = 1.2f;

    private readonly List<FamiliarData> owned = new List<FamiliarData>();

    public bool Owns(FamiliarData familiar)
    {
        return owned.Contains(familiar);
    }

    public void Add(FamiliarData familiar)
    {
        if (Owns(familiar))
        {
            return;
        }
        owned.Add(familiar);

        GameObject familiarObject = new GameObject(familiar.displayName);
        familiarObject.transform.SetParent(transform, false);
        familiarObject.transform.localPosition = Vector3.right * spacing * (owned.Count - 1);
        SpriteRenderer spriteRenderer = familiarObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = familiar.sprite;
        spriteRenderer.sortingOrder = 5;

        ApplyBonuses();
    }

    private void ApplyBonuses()
    {
        float jumpHeight = 1f;
        foreach (FamiliarData familiar in owned)
        {
            jumpHeight += familiar.jumpHeightBonus;
        }
        if (playerJumpBoost != null)
        {
            playerJumpBoost.SetHeightMultiplier(jumpHeight);
        }
    }
}
