using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // Chop on the beat: after a count-in, one cut line per beat. Press within the window around each beat.
    public class RhythmChopLogic
    {
        public enum Judgement { None, Perfect, Good, Miss }

        public float bpm = 100f;
        public int cuts = 8;
        public int countIn = 4;
        public float perfectWindow = 0.08f;
        public float goodWindow = 0.17f;

        public float Time { get; private set; }
        public float BeatLength => 60f / bpm;
        public int Beat => Mathf.FloorToInt(Time / BeatLength);
        public int NextCut { get; private set; }
        public int Perfects { get; private set; }
        public int Goods { get; private set; }
        public int Misses { get; private set; }
        public Judgement[] Results { get; }
        public bool Done => NextCut >= cuts;
        public float Score => cuts == 0 ? 1f : Mathf.Clamp01((Perfects + 0.6f * Goods) / cuts);

        public RhythmChopLogic(int cuts = 8)
        {
            this.cuts = cuts;
            Results = new Judgement[cuts];
        }

        public float CutTime(int cut) => (countIn + cut) * BeatLength;

        public void Tick(float dt)
        {
            Time += dt;
            // a cut the player let slide past its window is a miss
            while (!Done && Time > CutTime(NextCut) + goodWindow) Record(Judgement.Miss);
        }

        // Returns what the press counted as. None means it was too early to count for anything.
        public Judgement Press()
        {
            if (Done) return Judgement.None;
            float delta = Time - CutTime(NextCut);
            if (delta < -goodWindow) return Judgement.None;
            var result = Mathf.Abs(delta) <= perfectWindow ? Judgement.Perfect : Judgement.Good;
            Record(result);
            return result;
        }

        private void Record(Judgement result)
        {
            Results[NextCut] = result;
            if (result == Judgement.Perfect) Perfects++;
            else if (result == Judgement.Good) Goods++;
            else Misses++;
            NextCut++;
        }
    }
}
