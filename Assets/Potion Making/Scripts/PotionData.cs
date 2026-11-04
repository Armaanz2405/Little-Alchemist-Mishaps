using System.Collections.Generic;
using UnityEngine;

// One kind of potion (e.g. Springstep Draught). Create via Assets > Create > Potion Making > Potion.
[CreateAssetMenu(fileName = "NewPotion", menuName = "Potion Making/Potion")]
public class PotionData : ScriptableObject
{
    public string displayName = "New Potion";
    [Tooltip("Placeholder tint until we have art.")]
    public Color color = Color.white;
    [Tooltip("Optional. Leave empty to keep the prefab's sprite.")]
    public Sprite sprite;
    public List<IngredientTags> potionEffects = new List<IngredientTags>();

    public int ValueOf(IngredientTags.Effects effect)
    {
        int total = 0;
        foreach (var tag in potionEffects)
            if (tag != null && tag.tag_effect == effect) total += tag.tag_value;
        return total;
    }
}
