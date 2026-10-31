using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // Fishing-bar style: the ladle floats up while held and sinks when released, the gunk darts around. Keep them overlapped.
    public class CauldronFishingLogic
    {
        public int gunkCount = 3;
        public float timeLimit = 20f;
        public float ladleAccel = 2.4f;
        public float ladleMaxSpeed = 1.3f;
        public float ladleHalf = 0.11f;
        public float fillRate = 0.5f;
        public float drainRate = 0.3f;

        public float Ladle { get; private set; } = 0.2f;
        public float LadleVelocity { get; private set; }
        public float Gunk { get; private set; } = 0.6f;
        public float Catch { get; private set; } = 0.3f;
        public int Scooped { get; private set; }
        public float Elapsed { get; private set; }
        public bool Holding { get; private set; }
        public float TimeLeft => Mathf.Max(0f, timeLimit - Elapsed);
        public bool Overlapping => Mathf.Abs(Ladle - Gunk) <= ladleHalf;
        public bool Done => Scooped >= gunkCount || Elapsed >= timeLimit;
        public float Score => gunkCount == 0 ? 1f : Mathf.Clamp01((float)Scooped / gunkCount);

        private readonly System.Random rng;
        private float gunkTarget, gunkSpeed, retarget;

        public CauldronFishingLogic(int seed = 0)
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);
            NewGunkTarget();
        }

        // Returns true on the frame a piece of gunk gets scooped out.
        public bool Tick(float dt, bool holding)
        {
            if (Done) return false;
            Holding = holding;
            Elapsed += dt;

            LadleVelocity = Mathf.Clamp(LadleVelocity + (holding ? ladleAccel : -ladleAccel) * dt, -ladleMaxSpeed, ladleMaxSpeed);
            Ladle += LadleVelocity * dt;
            if (Ladle < 0f) { Ladle = 0f; LadleVelocity = Mathf.Max(0f, -LadleVelocity * 0.3f); }
            if (Ladle > 1f) { Ladle = 1f; LadleVelocity = Mathf.Min(0f, -LadleVelocity * 0.3f); }

            retarget -= dt;
            if (retarget <= 0f) NewGunkTarget();
            Gunk = Mathf.MoveTowards(Gunk, gunkTarget, gunkSpeed * dt);

            Catch = Mathf.Clamp01(Catch + (Overlapping ? fillRate : -drainRate) * dt);
            if (Catch < 1f) return false;
            Scooped++;
            Catch = 0.3f;
            Gunk = Mathf.Lerp(0.15f, 0.85f, (float)rng.NextDouble());
            NewGunkTarget();
            return true;
        }

        private void NewGunkTarget()
        {
            gunkTarget = Mathf.Lerp(0.08f, 0.92f, (float)rng.NextDouble());
            gunkSpeed = Mathf.Lerp(0.35f, 0.9f, (float)rng.NextDouble());
            retarget = Mathf.Lerp(0.7f, 1.6f, (float)rng.NextDouble());
        }
    }
}
