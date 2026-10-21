using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public class SliceLogic
    {
        public class Piece
        {
            public Vector2 pos;
            public Vector2 vel;
            public bool rotten;
            public bool sliced;
            public bool gone;
        }

        public int goodPieces = 8;
        public int rottenPieces = 2;
        public float spawnInterval = 0.7f;
        public float gravity = -900f;
        public float radius = 55f;
        public float floorY = -420f;
        public Rect area = new Rect(-450, -300, 900, 500);

        public readonly List<Piece> pieces = new List<Piece>();
        public int GoodSliced { get; private set; }
        public int RottenSliced { get; private set; }
        public bool Done => spawned >= goodPieces + rottenPieces && pieces.TrueForAll(p => p.gone);
        public float Score => goodPieces == 0 ? 1f : Mathf.Clamp01((GoodSliced - RottenSliced) / (float)goodPieces);

        private readonly System.Random rng;
        private readonly List<bool> queue = new List<bool>();
        private int spawned;
        private float timer;

        public SliceLogic(int seed = 0)
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);
            for (int i = 0; i < goodPieces; i++) queue.Add(false);
            for (int i = 0; i < rottenPieces; i++) queue.Insert(rng.Next(2, queue.Count + 1), true);
        }

        public void Tick(float dt)
        {
            timer -= dt;
            if (spawned < queue.Count && timer <= 0f)
            {
                Spawn(queue[spawned]);
                spawned++;
                timer = spawnInterval;
            }
            foreach (var p in pieces)
            {
                if (p.gone) continue;
                p.vel.y += gravity * dt;
                p.pos += p.vel * dt;
                if (p.pos.y < floorY) p.gone = true;
            }
        }

        public Piece Spawn(bool rotten)
        {
            float x = Mathf.Lerp(area.xMin + 100, area.xMax - 100, (float)rng.NextDouble());
            var p = new Piece
            {
                pos = new Vector2(x, floorY + 10),
                vel = new Vector2(Mathf.Lerp(-120, 120, (float)rng.NextDouble()) - x * 0.3f, Mathf.Lerp(900, 1050, (float)rng.NextDouble())),
                rotten = rotten
            };
            pieces.Add(p);
            return p;
        }

        // Swipe from a to b this frame. Returns how many pieces got cut.
        public int Swipe(Vector2 a, Vector2 b)
        {
            int cut = 0;
            foreach (var p in pieces)
            {
                if (p.gone || p.sliced) continue;
                if (DistanceToSegment(p.pos, a, b) > radius) continue;
                p.sliced = true;
                cut++;
                if (p.rotten) RottenSliced++; else GoodSliced++;
            }
            return cut;
        }

        public static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude < 1e-6f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
