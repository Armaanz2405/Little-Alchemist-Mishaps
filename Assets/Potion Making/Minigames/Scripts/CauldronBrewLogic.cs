using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // Aliyah's cauldron: each ingredient wants to be stirred a certain way (direction and speed) for a number of turns,
    // and only counts while the fire is in range. The fire dies down on its own, pumping the bellows brings it back.
    public class CauldronBrewLogic
    {
        public enum Direction { Clockwise, Counterclockwise }

        public class Need
        {
            public string name;
            public Color color;
            public Direction direction;
            public bool fast;
            public int turns;
            public float Done;   // turns completed so far
            public bool Complete => Done >= turns;
        }

        public float maxSpeed = 420f;        // degrees per second with the stick all the way over
        public float accel = 900f;
        public float decel = 700f;
        public float slowMin = 60f, slowMax = 200f;
        public float fastMin = 250f, fastMax = 440f;
        public float heatDecay = 0.085f;     // per second
        public float stokeAmount = 0.17f;    // one pump of the bellows
        public float heatMin = 0.42f, heatMax = 0.78f;
        public float scorchRate = 0.35f;     // scorch per second while too hot
        public float timeLimit = 40f;

        public IReadOnlyList<Need> Needs => needs;
        public int Current { get; private set; }
        public Need CurrentNeed => Current < needs.Count ? needs[Current] : null;
        public float Angle { get; private set; }
        public float Speed { get; private set; }      // signed, positive = counterclockwise
        public float Heat { get; private set; }
        public float Scorch { get; private set; }
        public float Elapsed { get; private set; }
        public int Fuel { get; }

        public float TimeLeft => Mathf.Max(0f, timeLimit - Elapsed);
        public bool FireTooLow => Heat < heatMin;
        public bool FireTooHot => Heat > heatMax;
        public bool FireOk => !FireTooLow && !FireTooHot;
        public bool Done => Current >= needs.Count || Elapsed >= timeLimit;

        public float Progress
        {
            get
            {
                if (needs.Count == 0) return 1f;
                float sum = 0f;
                foreach (var n in needs) sum += n.turns <= 0 ? 1f : Mathf.Clamp01(n.Done / n.turns);
                return sum / needs.Count;
            }
        }

        public float Score => Mathf.Clamp01(Progress * (1f - 0.5f * Mathf.Clamp01(Scorch)));

        private readonly List<Need> needs = new List<Need>();

        public CauldronBrewLogic(IEnumerable<Need> needs, int fuel = 0)
        {
            this.needs.AddRange(needs);
            Fuel = Mathf.Max(0, fuel);
            // flameflies burn hotter: each one makes the bellows a little stronger, and the fire starts higher
            stokeAmount += 0.012f * Mathf.Min(Fuel, 10);
            Heat = Mathf.Clamp(0.55f + 0.02f * Mathf.Min(Fuel, 10), 0f, heatMax);
        }

        // What an ingredient asks for, fixed per ingredient so players can learn it. Poorly refined ingredients take more turns.
        public static Need NeedFor(string name, Color color, float quality)
        {
            int h = 0;
            foreach (char c in name ?? "") h = h * 31 + c;
            h = Mathf.Abs(h);
            return new Need
            {
                name = name,
                color = color,
                direction = (h & 1) == 0 ? Direction.Clockwise : Direction.Counterclockwise,
                fast = ((h >> 1) & 1) == 1,
                turns = quality < 0.7f ? 3 : 2,
            };
        }

        public float DirectionSign(Direction d) => d == Direction.Counterclockwise ? 1f : -1f;
        public float BandMin(Need n) => n.fast ? fastMin : slowMin;
        public float BandMax(Need n) => n.fast ? fastMax : slowMax;

        public bool SpeedInBand(Need n) => n != null && Mathf.Abs(Speed) >= BandMin(n) && Mathf.Abs(Speed) <= BandMax(n);
        public bool DirectionRight(Need n) => n != null && Speed != 0f && Mathf.Sign(Speed) == DirectionSign(n.direction);
        public bool StirringRight => CurrentNeed != null && DirectionRight(CurrentNeed) && SpeedInBand(CurrentNeed);

        // stir: -1..1, negative = clockwise. stoke: true on the frame the bellows were pumped.
        public void Tick(float dt, float stir, bool stoke)
        {
            if (Done) return;
            Elapsed += dt;

            float target = Mathf.Clamp(stir, -1f, 1f) * maxSpeed;
            float rate = Mathf.Abs(target) > Mathf.Abs(Speed) ? accel : decel;
            Speed = Mathf.MoveTowards(Speed, target, rate * dt);
            Angle = Mathf.Repeat(Angle + Speed * dt, 360f);

            Heat = Mathf.Clamp01(Heat - heatDecay * dt + (stoke ? stokeAmount : 0f));
            if (FireTooHot) Scorch = Mathf.Clamp01(Scorch + scorchRate * dt);

            var need = CurrentNeed;
            if (need != null && FireOk && StirringRight)
            {
                need.Done += Mathf.Abs(Speed) * dt / 360f;
                if (need.Complete) Current++;
            }
        }
    }
}
