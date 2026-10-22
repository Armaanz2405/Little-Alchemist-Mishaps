using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Discovery
{
    [CreateAssetMenu(fileName = "IngredientCatalog", menuName = "Potion Making/Ingredient Catalog")]
    public class IngredientCatalog : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public IngredientData ingredient;
            [TextArea] public string hint;
        }

        public List<Entry> entries = new List<Entry>();

        public string HintFor(IngredientData ingredient)
        {
            foreach (var entry in entries)
                if (entry != null && entry.ingredient == ingredient) return entry.hint;
            return "";
        }

        public static string DescribeTags(IngredientData ingredient)
        {
            if (ingredient == null || ingredient.ingredientEffects == null) return "";
            var parts = new List<string>();
            foreach (var tag in ingredient.ingredientEffects)
            {
                if (tag == null || tag.tag_value <= 0) continue;
                string name = tag.tag_effect.ToString();
                parts.Add(char.ToUpper(name[0]) + name.Substring(1).ToLower() + " " + tag.tag_value);
            }
            return string.Join(", ", parts);
        }
    }
}
