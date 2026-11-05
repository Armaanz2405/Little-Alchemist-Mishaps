using System.Collections.Generic;
using Nocturne.PotionMaking.Minigames;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CuttingBoard : MonoBehaviour
{
    [SerializeField] private Ingredient ingredientPrefab;
    [Tooltip("Where the packed ingredient pops out. Defaults to just above the mortar.")]
    [SerializeField] private Transform ingredientSpawnPoint;
    [SerializeField] private SpriteRenderer ingredientOnBoardSprite;

    [SerializeField] private MinigameStation chopMinigameStation;

    private IngredientData content;
    public bool HasIngredient => content != null;
    //public IReadOnlyIngredientData Content => content;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;


    void Start()
    {
        if (ingredientOnBoardSprite != null) ingredientOnBoardSprite.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ingredient = other.GetComponentInParent<Ingredient>();
        if (content != null) return;
        if (ingredient == null || ingredient.data == null || ingredient.data.ingredientEffects.Count <= 0 || !ingredient.TryUse()) return;
        AddIngredient(ingredient.data);
        Destroy(ingredient.gameObject);
    }

    public IngredientData AddIngredient(IngredientData ingredient)
    {
        content = ingredient;
        Debug.Log($"[Chopping Board] Added {ingredient.displayName}");
        //onIngredientAdded.Invoke(ingredient);

        if (chopMinigameStation != null)
        {
            chopMinigameStation.isReadyToPlay = true;
            // the knife needs to know the tags so the player can pick which one to cut away
            if (chopMinigameStation.Game is KnifeSliceMinigame knife) knife.SetIngredient(ingredient);
        }
        // display the ingredient
        if (ingredientOnBoardSprite != null)
        {
            ingredientOnBoardSprite.color = ingredient.color;
            if (ingredient.sprite != null) ingredientOnBoardSprite.sprite = ingredient.sprite;
            ingredientOnBoardSprite.gameObject.SetActive(true);
        }

        return null;
    }

    public void Chop_Trigger()
    {
        Chop();
    }

    // Removes the tag the player picked in the minigame (the first one when there was no choice).
    // How well they chopped becomes the chopped ingredient's refine quality.
    private IngredientData Chop()
    {
        if (content == null) return null;
        var knife = chopMinigameStation != null ? chopMinigameStation.Game as KnifeSliceMinigame : null;
        IngredientData choppedIngredient = ScriptableObject.CreateInstance<IngredientData>();
        List<IngredientTags> new_tags = new List<IngredientTags>(content.ingredientEffects);
        int index = knife != null ? Mathf.Clamp(knife.ChosenTag, 0, new_tags.Count - 1) : 0;
        IngredientTags removed = new_tags[index];
        new_tags.RemoveAt(index);
        float score = knife != null ? knife.LastScore : 1f;

        choppedIngredient.displayName = $"Chopped {content.displayName}";
        choppedIngredient.ingredientEffects = new_tags;
        choppedIngredient.color = new_tags.Count > 0 ? IngredientTags.ColorOf(new_tags[0].tag_effect) : content.color;
        choppedIngredient.refineQuality = CraftQuality.Combine(score, content.refineQuality);
        Debug.Log($"[Chopping Board] Cut {IngredientTags.Name(removed.tag_effect)} off {content.displayName}, quality {Mathf.RoundToInt(choppedIngredient.refineQuality * 100)}%");

        if (ingredientOnBoardSprite != null) ingredientOnBoardSprite.gameObject.SetActive(false);
        content = null;

        if (ingredientPrefab != null)
        {
            Vector3 at = ingredientSpawnPoint != null ? ingredientSpawnPoint.position : transform.position + Vector3.up;
            Ingredient ingredient = Instantiate(ingredientPrefab, at, Quaternion.identity);
            ingredient.data = choppedIngredient;
            ingredient.ApplyLook();
        }

        return choppedIngredient;
    }
}
