using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // How well something was made: the station minigame score times the quality of what went into it.
    public static class CraftQuality
    {
        public static float Combine(float minigameScore, IEnumerable<float> inputs)
        {
            float sum = 0f;
            int n = 0;
            foreach (var q in inputs)
            {
                sum += Mathf.Clamp01(q);
                n++;
            }
            float average = n > 0 ? sum / n : 1f;
            return Mathf.Clamp01(minigameScore) * average;
        }

        public static float Combine(float minigameScore, params float[] inputs) => Combine(minigameScore, (IEnumerable<float>)inputs);

        // How much of a potion's score survives. Weight 0 ignores craft, weight 1 scales the score by it.
        public static float Multiplier(float quality, float weight)
        {
            float w = Mathf.Clamp01(weight);
            return 1f - w + w * Mathf.Clamp01(quality);
        }
    }
}
