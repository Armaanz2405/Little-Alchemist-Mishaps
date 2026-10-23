using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientTags {
    public enum Effects {
        WARMING,
        CHILLING,
        HEALING,
        POISON,
        GLOWING,
        BLINDING,
        REVEALING,
        SILENCING,
        WITHERING,
        CRYSTALLIZING,
        BUBBLING,
        STRENGTHENING
    }
    public Effects tag_effect;
    [Range(1,4)]
    public int tag_value;

    public static string Name(Effects effect)
    {
        string s = effect.ToString();
        return char.ToUpper(s[0]) + s.Substring(1).ToLower();
    }

    public static Color ColorOf(Effects effect)
    {
        switch (effect)
        {
            case Effects.WARMING: return new Color(1f, 0.55f, 0.2f);
            case Effects.CHILLING: return new Color(0.55f, 0.85f, 1f);
            case Effects.HEALING: return new Color(0.45f, 0.9f, 0.45f);
            case Effects.POISON: return new Color(0.55f, 0.75f, 0.2f);
            case Effects.GLOWING: return new Color(1f, 0.95f, 0.45f);
            case Effects.BLINDING: return new Color(0.95f, 0.95f, 0.9f);
            case Effects.REVEALING: return new Color(0.4f, 0.9f, 0.8f);
            case Effects.SILENCING: return new Color(0.5f, 0.55f, 0.75f);
            case Effects.WITHERING: return new Color(0.55f, 0.4f, 0.3f);
            case Effects.CRYSTALLIZING: return new Color(0.85f, 0.6f, 1f);
            case Effects.BUBBLING: return new Color(1f, 0.6f, 0.85f);
            case Effects.STRENGTHENING: return new Color(0.9f, 0.25f, 0.25f);
            default: return Color.white;
        }
    }
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
