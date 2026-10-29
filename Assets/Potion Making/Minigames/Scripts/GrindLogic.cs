using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // Button mash: alternate left and right presses to grind before the time runs out.
    public class GrindLogic
    {
        public enum Side { None, Left, Right }

        public int strokesNeeded = 14;
        public float timeLimit = 8f;

        public int Strokes { get; private set; }
        public float TimeLeft { get; private set; }
        public Side LastSide { get; private set; } = Side.None;
        public float Progress => strokesNeeded == 0 ? 1f : Mathf.Clamp01((float)Strokes / strokesNeeded);
        public bool Done => Strokes >= strokesNeeded || TimeLeft <= 0f;
        public bool Succeeded => Strokes >= strokesNeeded;
        public float Score => Progress;

        public GrindLogic() => TimeLeft = timeLimit;

        public void Tick(float dt)
        {
            if (!Done) TimeLeft = Mathf.Max(0f, TimeLeft - dt);
        }

        // Returns false when the press didn't count (same side twice, or already done).
        public bool Press(Side side)
        {
            if (Done || side == Side.None || side == LastSide) return false;
            LastSide = side;
            Strokes++;
            return true;
        }
    }
}
