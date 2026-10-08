using UnityEngine;

// One kind of ingredient (e.g. Bubblecap). Create via Assets > Create > Potion Making > Ingredient.
[CreateAssetMenu(fileName = "NewIngredient", menuName = "Potion Making/Ingredient")]
public class IngredientData : ScriptableObject
{
    public string displayName = "New Ingredient";
    [Tooltip("Placeholder tint until we have art.")]
    public Color color = Color.white;
    [Tooltip("Optional. Leave empty to keep the prefab's sprite.")]
    public Sprite sprite;
}
