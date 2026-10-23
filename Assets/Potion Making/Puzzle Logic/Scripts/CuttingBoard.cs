using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CuttingBoard : MonoBehaviour
{
    [SerializeField] private Ingredient ingredientPrefab;
    [Tooltip("Where the packed ingredient pops out. Defaults to just above the mortar.")]
    [SerializeField] private Transform ingredientSpawnPoint;
    [SerializeField] private SpriteRenderer ingredientOnBoardSprite;

    [SerializeField] private Nocturne.PotionMaking.Minigames.MinigameStation chopMinigameStation;

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

        if (chopMinigameStation != null) chopMinigameStation.isReadyToPlay = true;
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

    private IngredientData Chop()
    {
        if (content == null) return null;
        IngredientData choppedIngredient = ScriptableObject.CreateInstance<IngredientData>();
        List<IngredientTags> new_tags = new List<IngredientTags>(content.ingredientEffects);
        new_tags.RemoveAt(0);

        choppedIngredient.displayName = $"Chopped {content.displayName}";
        choppedIngredient.ingredientEffects = new_tags;
        choppedIngredient.color = new_tags.Count > 0 ? IngredientTags.ColorOf(new_tags[0].tag_effect) : content.color;

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
