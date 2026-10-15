using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.TagBrewing
{
    [CreateAssetMenu(fileName = "TagRules", menuName = "Potion Making/Tag Rules")]
    public class TagRules : ScriptableObject
    {
        [System.Serializable]
        public class OpposingPair
        {
            public IngredientTags.Effects a;
            public IngredientTags.Effects b;
        }

        [System.Serializable]
        public class EffectColor
        {
            public IngredientTags.Effects effect;
            public Color color = Color.white;
        }

        public List<OpposingPair> opposing = new List<OpposingPair>();

        [Header("Bubbling over (risk, not a hard stop)")]
        [Min(1)] public int safeIngredientCount = 4;
        [Range(0f, 1f)] public float overflowChancePerExtra = 0.25f;

        [Header("Potion tint by strongest tag")]
        public List<EffectColor> colors = new List<EffectColor>();

        public Color ColorFor(IngredientTags.Effects effect)
        {
            foreach (var c in colors)
                if (c != null && c.effect == effect) return c.color;
            return new Color(0.7f, 0.6f, 0.8f);
        }
    }
}
