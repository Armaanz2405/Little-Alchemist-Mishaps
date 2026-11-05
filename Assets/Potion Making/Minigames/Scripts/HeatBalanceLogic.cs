using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // The brew heats up on its own and stirring cools it. Keep the heat inside a band that keeps drifting.
    public class HeatBalanceLogic
    {
        public float heatRate = 0.3f;
        public float coolRate = 0.42f;
        public float bandHalf = 0.09f;
        public float bandSpeed = 0.22f;
        public float needed = 7f;
        public float timeLimit = 22f;

        public float Heat { get; private set; } = 0.3f;
        public float Band { get; private set; }
        public float Target { get; private set; }
        public float InBandTime { get; private set; }
        public float Elapsed { get; private set; }
        public bool Stirring { get; private set; }
        public float TimeLeft => Mathf.Max(0f, timeLimit - Elapsed);
        public bool InBand => Mathf.Abs(Heat - Band) <= bandHalf;
        public float Progress => needed <= 0f ? 1f : Mathf.Clamp01(InBandTime / needed);
        public bool Done => Progress >= 1f || Elapsed >= timeLimit;
        public float Score => Progress;

        private readonly System.Random rng;
        private float retarget;

        public HeatBalanceLogic(int seed = 0)
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);
            Band = Target = 0.5f;
            NewTarget();
        }

        public void Tick(float dt, bool stirring)
        {
            if (Done) return;
            Stirring = stirring;
            Elapsed += dt;
            Heat = Mathf.Clamp01(Heat + (stirring ? -coolRate : heatRate) * dt);
            Band = Mathf.MoveTowards(Band, Target, bandSpeed * dt);
            if (InBand) InBandTime += dt;
            retarget -= dt;
            if (retarget <= 0f) NewTarget();
        }

        private void NewTarget()
        {
            Target = Mathf.Lerp(0.2f, 0.8f, (float)rng.NextDouble());
            retarget = Mathf.Lerp(2.5f, 4f, (float)rng.NextDouble());
        }
    }
}
