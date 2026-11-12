using System.Collections.Generic;
using Nocturne.PotionMaking.Minigames;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AlchemyStation : MonoBehaviour
{
    [SerializeField] private Ingredient ingredientPrefab;
    [Tooltip("Where the packed ingredient pops out. Defaults to just above the mortar.")]
    [SerializeField] private Transform ingredientSpawnPoint;

    private IngredientData content;
    public bool HasIngredient => content != null;
    //public IReadOnlyIngredientData Content => content;
    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

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
        Debug.Log($"[Alchemy Station] Added {ingredient.displayName}");

        return null;
    }

    public void TakeIngredient()
    {
        //
    }

    public void Transmutate_Trigger()
    {
        Transmutate();
    }

    private IngredientData Transmutate()
    {
        if (content == null) return null;

        IngredientData transedIngredient = ScriptableObject.CreateInstance<IngredientData>();
        List<IngredientTags> new_tags = new List<IngredientTags>();

        //transmutate ingredient tags here
        foreach (IngredientTags tag in content.ingredientEffects)
        {
            int current_effect_i = System.Array.IndexOf(System.Enum.GetValues(typeof(IngredientTags.Effects)),tag.tag_effect);
            int new_effect_i = (current_effect_i + 3) % System.Enum.GetNames(typeof(IngredientTags.Effects)).Length;
            //tag.tag_effect = (IngredientTags.Effects)new_effect_i;
            new_tags.Add(new IngredientTags { tag_effect = (IngredientTags.Effects)new_effect_i, tag_value = tag.tag_value });
            Debug.Log(new_tags[new_tags.Count - 1].tag_effect);
        }

        bool hasTags = new_tags.Count > 0;
        transedIngredient.displayName = $"Transfigured {content.displayName}";
        transedIngredient.ingredientEffects = new_tags;
        transedIngredient.color = hasTags ? IngredientTags.ColorOf(new_tags[0].tag_effect) : Color.black;

        if (ingredientPrefab != null)
        {
            Vector3 at = ingredientSpawnPoint != null ? ingredientSpawnPoint.position : transform.position + Vector3.up;
            Ingredient ingredient = Instantiate(ingredientPrefab, at, Quaternion.identity);
            ingredient.data = transedIngredient;
            ingredient.ApplyLook();
        }

        content = null;
        return transedIngredient;
    }
}
