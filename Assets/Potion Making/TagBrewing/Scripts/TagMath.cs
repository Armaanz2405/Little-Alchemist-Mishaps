using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nocturne.PotionMaking.TagBrewing
{
    public static class TagMath
    {
        public static Dictionary<IngredientTags.Effects, int> Sum(IEnumerable<IngredientData> ingredients)
        {
            var totals = new Dictionary<IngredientTags.Effects, int>();
            foreach (var ingredient in ingredients)
            {
                if (ingredient == null || ingredient.ingredientEffects == null) continue;
                foreach (var tag in ingredient.ingredientEffects)
                {
                    if (tag == null || tag.tag_value <= 0) continue;
                    totals.TryGetValue(tag.tag_effect, out int v);
                    totals[tag.tag_effect] = v + tag.tag_value;
                }
            }
            return totals;
        }

        public static Dictionary<IngredientTags.Effects, int> Combine(IEnumerable<IngredientData> ingredients, TagRules rules)
        {
            var totals = Sum(ingredients);
            if (rules != null)
            {
                foreach (var pair in rules.opposing)
                {
                    if (pair == null || pair.a == pair.b) continue;
                    totals.TryGetValue(pair.a, out int a);
                    totals.TryGetValue(pair.b, out int b);
                    int cancel = Mathf.Min(a, b);
                    if (cancel == 0) continue;
                    totals[pair.a] = a - cancel;
                    totals[pair.b] = b - cancel;
                }
            }
            return totals.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public static IngredientTags.Effects? Dominant(Dictionary<IngredientTags.Effects, int> tags)
        {
            if (tags == null || tags.Count == 0) return null;
            return tags.OrderByDescending(kv => kv.Value).ThenBy(kv => (int)kv.Key).First().Key;
        }

        public static string Describe(Dictionary<IngredientTags.Effects, int> tags)
        {
            if (tags == null || tags.Count == 0) return "";
            return string.Join("  ·  ", tags.OrderByDescending(kv => kv.Value).ThenBy(kv => (int)kv.Key)
                .Select(kv => $"{Nice(kv.Key)} {kv.Value}"));
        }

        public static string Nice(IngredientTags.Effects effect)
        {
            string s = effect.ToString();
            return char.ToUpper(s[0]) + s.Substring(1).ToLower();
        }

        public static float OverflowChance(int count, TagRules rules)
        {
            if (rules == null) return 0f;
            int extra = count - rules.safeIngredientCount;
            return extra <= 0 ? 0f : Mathf.Clamp01(extra * rules.overflowChancePerExtra);
        }
    }
}
