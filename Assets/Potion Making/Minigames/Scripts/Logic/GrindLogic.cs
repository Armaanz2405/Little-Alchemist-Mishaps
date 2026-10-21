using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    public class GrindLogic
    {
        public enum Side { None, Left, Right }

        public int strokesNeeded = 16;
        public float timeLimit = 10f;

        public int Strokes { get; private set; }
        public float TimeLeft { get; private set; }
        public Side LastSide { get; private set; } = Side.None;
        public float Progress => Mathf.Clamp01((float)Strokes / strokesNeeded);
        public bool Done => Strokes >= strokesNeeded || TimeLeft <= 0f;
        public bool Succeeded => Strokes >= strokesNeeded;
        public float Score => Progress;

        public GrindLogic() => TimeLeft = timeLimit;

        public void Reset()
        {
            Strokes = 0;
            TimeLeft = timeLimit;
            LastSide = Side.None;
        }

        public void Tick(float dt)
        {
            if (!Done) TimeLeft = Mathf.Max(0f, TimeLeft - dt);
        }

        public bool Press(Side side)
        {
            if (Done || side == Side.None || side == LastSide) return false;
            LastSide = side;
            Strokes++;
            return true;
        }
    }
}
