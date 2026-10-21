using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public class PourLogic
    {
        public float pourRate = 0.35f;
        public float targetMin = 0.72f;
        public float targetMax = 0.86f;

        public float Fill { get; private set; }
        public bool Pouring { get; private set; }
        public bool Released { get; private set; }
        public bool Spilled => Fill >= 1f;
        public bool Done => Released || Spilled;
        public bool InTarget => Fill >= targetMin && Fill <= targetMax;

        public float Score
        {
            get
            {
                if (Spilled || Fill <= 0f) return 0f;
                if (InTarget) return 1f;
                float miss = Fill < targetMin ? targetMin - Fill : Fill - targetMax;
                return Mathf.Clamp01(1f - miss * 4f);
            }
        }

        public void Tick(float dt, bool holding)
        {
            if (Done) return;
            if (holding)
            {
                Pouring = true;
                Fill = Mathf.Min(1f, Fill + pourRate * dt);
            }
            else if (Pouring)
            {
                Released = true;
            }
        }
    }
}
