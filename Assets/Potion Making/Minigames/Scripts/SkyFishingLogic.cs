using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // Bugs drift around the night sky. A lit lantern pulls in the ones nearby and catches them when they reach it.
    public class SkyFishingLogic
    {
        public class Bug
        {
            public Vector2 pos;
            public Vector2 vel;
            public float phase;
            public bool caught;
        }

        public Rect area = new Rect(-480, -230, 960, 460);
        public int total = 10;
        public float spawnInterval = 1.1f;
        public float timeLimit = 16f;
        public float lanternSpeed = 420f;
        public float lightRadius = 230f;
        public float pull = 240f;
        public float catchRadius = 44f;

        public readonly List<Bug> bugs = new List<Bug>();
        public float LanternX { get; private set; }
        public bool Lit { get; private set; }
        public int Caught { get; private set; }
        public int Spawned { get; private set; }
        public float Elapsed { get; private set; }
        public float TimeLeft => Mathf.Max(0f, timeLimit - Elapsed);
        public Vector2 LanternPos => new Vector2(LanternX, area.yMin + 45f);
        public bool Done => Caught >= total || Elapsed >= timeLimit;
        public float Score => total == 0 ? 1f : Mathf.Clamp01((float)Caught / total);

        private readonly System.Random rng;
        private float spawnTimer = 0.3f;

        public SkyFishingLogic(int seed = 0)
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);
        }

        // Returns how many bugs got caught this frame.
        public int Tick(float dt, float move, bool lit)
        {
            if (Done) return 0;
            Elapsed += dt;
            Lit = lit;
            LanternX = Mathf.Clamp(LanternX + Mathf.Clamp(move, -1f, 1f) * lanternSpeed * dt, area.xMin + 60f, area.xMax - 60f);

            spawnTimer -= dt;
            if (spawnTimer <= 0f && Spawned < total)
            {
                bugs.Add(new Bug
                {
                    pos = new Vector2(Mathf.Lerp(area.xMin + 60f, area.xMax - 60f, (float)rng.NextDouble()), Mathf.Lerp(area.yMin + 160f, area.yMax - 40f, (float)rng.NextDouble())),
                    vel = new Vector2(Mathf.Lerp(-70f, 70f, (float)rng.NextDouble()), Mathf.Lerp(-30f, 30f, (float)rng.NextDouble())),
                    phase = (float)rng.NextDouble() * 6.28f,
                });
                Spawned++;
                spawnTimer = spawnInterval;
            }

            int caughtNow = 0;
            Vector2 lantern = LanternPos;
            foreach (var bug in bugs)
            {
                if (bug.caught) continue;
                bug.vel += new Vector2(Mathf.Sin(Elapsed * 1.7f + bug.phase) * 140f, Mathf.Cos(Elapsed * 1.3f + bug.phase) * 70f) * dt;
                bug.vel = Vector2.ClampMagnitude(bug.vel, 95f);
                bug.pos += bug.vel * dt;
                if (bug.pos.x < area.xMin + 20f || bug.pos.x > area.xMax - 20f) bug.vel.x = -bug.vel.x;
                if (bug.pos.y < area.yMin + 120f || bug.pos.y > area.yMax - 20f) bug.vel.y = -bug.vel.y;
                bug.pos.x = Mathf.Clamp(bug.pos.x, area.xMin + 20f, area.xMax - 20f);
                bug.pos.y = Mathf.Clamp(bug.pos.y, area.yMin + 120f, area.yMax - 20f);

                if (lit && Vector2.Distance(bug.pos, lantern) <= lightRadius)
                {
                    bug.pos = Vector2.MoveTowards(bug.pos, lantern + new Vector2(0, 30f), pull * dt);
                    if (Vector2.Distance(bug.pos, lantern + new Vector2(0, 30f)) <= catchRadius)
                    {
                        bug.caught = true;
                        Caught++;
                        caughtNow++;
                    }
                }
            }
            return caughtNow;
        }
    }
}
