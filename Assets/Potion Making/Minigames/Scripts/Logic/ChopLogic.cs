using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public class ChopLogic
    {
        public float[] targets = { 0.3f, 0.65f, 0.45f };
        public float startSpeed = 0.6f;
        public float speedUpPerCut = 0.25f;
        public float tolerance = 0.06f;

        private float t;
        private readonly List<bool> results = new List<bool>();

        public int CutIndex => results.Count;
        public int CutsTotal => targets.Length;
        public int Hits { get { int h = 0; foreach (var r in results) if (r) h++; return h; } }
        public IReadOnlyList<bool> Results => results;
        public bool Done => results.Count >= targets.Length;
        public float Score => targets.Length == 0 ? 1f : (float)Hits / targets.Length;
        public float CurrentTarget => Done ? -1f : targets[results.Count];
        public float KnifePosition => Mathf.PingPong(t, 1f);
        public float Speed => startSpeed + speedUpPerCut * results.Count;

        public void Tick(float dt)
        {
            if (!Done) t += dt * Speed * 2f;
        }

        public bool Chop()
        {
            if (Done) return false;
            bool hit = Mathf.Abs(KnifePosition - CurrentTarget) <= tolerance;
            results.Add(hit);
            return hit;
        }

        public void SetKnife(float position) => t = Mathf.Clamp01(position);
    }
}
