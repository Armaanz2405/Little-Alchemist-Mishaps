using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // What the grind screen needs from the mortar.
    public interface IGrindSource
    {
        IReadOnlyList<IngredientData> Contents { get; }
        int Capacity { get; }
        // grinding costs energy; looking and taking things out doesn't
        bool CanAffordGrind { get; }
        // takes one ingredient back out of the mortar and hands it to the player. False when their hands are full.
        bool Eject(int index);
    }

    // What grinding a set of ingredients together makes. The mortar packs with this too, so the screen can't drift from the result.
    public static class MortarPreview
    {
        public class Result
        {
            public List<IngredientTags> outcome = new List<IngredientTags>();
            // tags more than one ingredient brings, so they stack
            public List<IngredientTags.Effects> enhanced = new List<IngredientTags.Effects>();
            // nothing cancels under the current tag rules; the screen has the row ready for when something does
            public List<IngredientTags.Effects> removed = new List<IngredientTags.Effects>();
            public string name = "New Goop";
            public Color color = Color.black;
        }

        public static Result Of(IReadOnlyList<IngredientData> contents)
        {
            var result = new Result();
            var totals = new Dictionary<IngredientTags.Effects, int>();
            var sources = new Dictionary<IngredientTags.Effects, int>();
            if (contents != null)
                foreach (var ingredient in contents)
                    foreach (var effect in Effects(ingredient).Distinct())
                    {
                        totals.TryGetValue(effect, out int total);
                        totals[effect] = total + ingredient.ingredientEffects.Where(t => t != null && t.tag_effect == effect && t.tag_value > 0).Sum(t => t.tag_value);
                        sources.TryGetValue(effect, out int count);
                        sources[effect] = count + 1;
                    }

            result.outcome = totals
                .Select(kv => new IngredientTags { tag_effect = kv.Key, tag_value = kv.Value })
                .OrderByDescending(tag => tag.tag_value).ThenBy(tag => (int)tag.tag_effect)
                .ToList();
            result.enhanced = sources.Where(kv => kv.Value > 1).Select(kv => kv.Key).OrderBy(e => (int)e).ToList();
            if (result.outcome.Count > 0)
            {
                result.name = $"{IngredientTags.Name(result.outcome[0].tag_effect)} Paste";
                result.color = IngredientTags.ColorOf(result.outcome[0].tag_effect);
            }
            return result;
        }

        // the tags this ingredient shares with something else in the mortar
        public static List<IngredientTags.Effects> SharedTags(IReadOnlyList<IngredientData> contents, int index)
        {
            var shared = new List<IngredientTags.Effects>();
            if (contents == null || index < 0 || index >= contents.Count) return shared;
            foreach (var effect in Effects(contents[index]).Distinct())
                for (int i = 0; i < contents.Count; i++)
                    if (i != index && Effects(contents[i]).Contains(effect))
                    {
                        shared.Add(effect);
                        break;
                    }
            return shared;
        }

        private static IEnumerable<IngredientTags.Effects> Effects(IngredientData ingredient)
        {
            if (ingredient == null || ingredient.ingredientEffects == null) return Enumerable.Empty<IngredientTags.Effects>();
            return ingredient.ingredientEffects.Where(t => t != null && t.tag_value > 0).Select(t => t.tag_effect);
        }
    }
}
