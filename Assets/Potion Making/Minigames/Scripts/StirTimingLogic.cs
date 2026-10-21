using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public class StirTimingLogic
    {
        public int stirsNeeded = 5;
        public float startSpeed = 140f;
        public float speedUp = 35f;
        public float startZone = 70f;
        public float zoneShrink = 8f;
        public float minZone = 30f;

        public float Angle { get; private set; }
        public float ZoneStart { get; private set; }
        public float ZoneSize { get; private set; }
        public float Speed { get; private set; }
        public int Hits { get; private set; }
        public int Misses { get; private set; }
        public IReadOnlyList<bool> Results => results;
        public bool Done => results.Count >= stirsNeeded;
        public float Score => stirsNeeded == 0 ? 1f : (float)Hits / stirsNeeded;

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
            }
            else Misses++;
            NewZone();
            return hit;
        }

        public void SetAngle(float angle) => Angle = Mathf.Repeat(angle, 360f);

        private void NewZone()
        {
            ZoneStart = Mathf.Repeat(Angle + 90f + (float)rng.NextDouble() * 180f, 360f);
        }
    }
}
