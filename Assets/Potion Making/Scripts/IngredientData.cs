using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientTags {
    public enum Effects {
        POISON,
        SLEEPY,
        STRENGTH,
        CORPOREAL,
        BUBBLY
    }
    public Effects tag_effect;
    [Range(1,4)]
    public int tag_value;
}
// One kind of ingredient (e.g. Bubblecap). Create via Assets > Create > Potion Making > Ingredient.
[CreateAssetMenu(fileName = "NewIngredient", menuName = "Potion Making/Ingredient")]
public class IngredientData : ScriptableObject
{
    

    public string displayName = "New Ingredient";
    public List<IngredientTags> ingredientEffects = new List<IngredientTags>();
    [Tooltip("Placeholder tint until we have art.")]
    public Color color = Color.white;
    [Tooltip("Optional. Leave empty to keep the prefab's sprite.")]
    public Sprite sprite;
}
