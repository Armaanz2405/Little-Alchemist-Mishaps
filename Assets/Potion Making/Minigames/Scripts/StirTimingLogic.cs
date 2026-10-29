using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public class StirTimingLogic
    {
        public enum Result { Hit, Miss, Burn }

        public int stirsNeeded = 5;
        public float startSpeed = 140f;
        public float speedUp = 35f;
        public float startZone = 70f;
        public float zoneShrink = 8f;
        public float minZone = 30f;
        // the red part of the ring: stirring there scorches the brew and undoes a good stir
        public float burnZone = 45f;

        public float Angle { get; private set; }
        public float ZoneStart { get; private set; }
        public float ZoneSize { get; private set; }
        public float BurnStart { get; private set; }
        public float BurnSize => burnZone;
        public float Speed { get; private set; }
        public int Hits { get; private set; }
        public int Misses { get; private set; }
        public int Burns { get; private set; }
        public Result Last { get; private set; } = Result.Miss;
        public IReadOnlyList<bool> Results => results;
        public bool Done => results.Count >= stirsNeeded;
        public float Score => stirsNeeded == 0 ? 1f : Mathf.Clamp01((Hits - Burns) / (float)stirsNeeded);

        private readonly List<bool> results = new List<bool>();
        private readonly System.Random rng;

        public StirTimingLogic(int seed = 0)
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);
            Speed = startSpeed;
            ZoneSize = startZone;
            NewZone();
        }

        public void Tick(float dt)
        {
            if (!Done) Angle = Mathf.Repeat(Angle + Speed * dt, 360f);
        }

        public bool InZone => Mathf.Repeat(Angle - ZoneStart, 360f) <= ZoneSize;
        public bool InBurnZone => burnZone > 0f && Mathf.Repeat(Angle - BurnStart, 360f) <= burnZone;

        public bool Stir()
        {
            if (Done) return false;
            bool hit = InZone;
            results.Add(hit);
            if (hit)
            {
                Hits++;
                Speed += speedUp;
                ZoneSize = Mathf.Max(minZone, ZoneSize - zoneShrink);
                Last = Result.Hit;
            }
            else if (InBurnZone)
            {
                Misses++;
                Burns++;
                Last = Result.Burn;
            }
            else
            {
                Misses++;
                Last = Result.Miss;
            }
            NewZone();
            return hit;
        }

        public void SetAngle(float angle) => Angle = Mathf.Repeat(angle, 360f);

        private void NewZone()
        {
            ZoneStart = Mathf.Repeat(Angle + 90f + (float)rng.NextDouble() * 180f, 360f);
            // the burn zone goes somewhere in the gap after the green zone, never touching it
            float gap = Mathf.Max(0f, 360f - ZoneSize - burnZone - 40f);
            BurnStart = Mathf.Repeat(ZoneStart + ZoneSize + 20f + (float)rng.NextDouble() * gap, 360f);
        }
    }
}
