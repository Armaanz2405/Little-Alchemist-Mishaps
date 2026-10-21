using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public class StirLogic
    {
        public float turnsNeeded = 6f;
        public float calmMinSpeed = 0.25f;
        public float calmMaxSpeed = 1.4f;
        public float bubblingMinSpeed = 1.6f;
        public float overflowRise = 0.45f;
        public float overflowFall = 0.15f;
        public Vector2[] bubblingWindows = { new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.85f) };

        public float Progress { get; private set; }
        public float Overflow { get; private set; }
        public float Speed { get; private set; }
        public bool Done => Progress >= 1f || Overflow >= 1f;
        public bool Succeeded => Progress >= 1f && Overflow < 1f;
        public float Score => Succeeded ? Mathf.Clamp01(1f - Overflow) : 0f;

        public bool Bubbling
        {
            get
            {
                foreach (var w in bubblingWindows)
                    if (Progress >= w.x && Progress < w.y) return true;
                return false;
            }
        }

        public bool InBand(float speed) => Bubbling ? speed >= bubblingMinSpeed : speed >= calmMinSpeed && speed <= calmMaxSpeed;

        public void Tick(float dt, float angleDelta)
        {
            if (Done || dt <= 0f) return;
            float turns = Mathf.Abs(angleDelta) / (2f * Mathf.PI);
            Speed = Mathf.Lerp(Speed, turns / dt, Mathf.Clamp01(dt * 8f));

            bool stirring = Speed > 0.05f;
            if (InBand(Speed))
            {
                Progress = Mathf.Min(1f, Progress + turns / turnsNeeded);
                Overflow = Mathf.Max(0f, Overflow - overflowFall * dt);
            }
            else if (stirring || Bubbling)
            {
                Overflow = Mathf.Min(1f, Overflow + overflowRise * dt);
            }
        }
    }
}
