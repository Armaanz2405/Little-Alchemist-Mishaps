using Nocturne.PotionMaking.Minigames;
using NUnit.Framework;
using UnityEngine;

public class MinigameTests
{
    [Test]
    public void Swipe_CutsPiecesItPassesThrough()
    {
        var s = new SliceLogic(1);
        var good = s.Spawn(false); good.pos = new Vector2(0, 0);
        var rotten = s.Spawn(true); rotten.pos = new Vector2(400, 0);
        Assert.AreEqual(1, s.Swipe(new Vector2(-100, 0), new Vector2(100, 0)));
        Assert.IsTrue(good.sliced);
        Assert.IsFalse(rotten.sliced);
        Assert.AreEqual(1, s.GoodSliced);
    }

    [Test]
    public void Swipe_CantCutTheSamePieceTwice()
    {
        var s = new SliceLogic(1);
        var p = s.Spawn(false); p.pos = Vector2.zero;
        s.Swipe(new Vector2(-100, 0), new Vector2(100, 0));
        Assert.AreEqual(0, s.Swipe(new Vector2(-100, 0), new Vector2(100, 0)));
    }

    [Test]
    public void RottenCuts_LowerTheScore()
    {
        var s = new SliceLogic(1) { goodPieces = 2 };
        var a = s.Spawn(false); a.pos = new Vector2(0, 0);
        var b = s.Spawn(false); b.pos = new Vector2(0, 200);
        var r = s.Spawn(true); r.pos = new Vector2(0, 400);
        s.Swipe(new Vector2(0, -50), new Vector2(0, 450));
        Assert.AreEqual(0.5f, s.Score, 1e-4);
    }

    [Test]
    public void Pieces_FallAndTheGameEnds()
    {
        var s = new SliceLogic(1);
        for (int i = 0; i < 60 * 30 && !s.Done; i++) s.Tick(1f / 60f);
        Assert.IsTrue(s.Done);
        Assert.AreEqual(s.goodPieces + s.rottenPieces, s.pieces.Count);
    }

    [Test]
    public void Stir_HitInZoneCounts()
    {
        var s = new StirTimingLogic(1);
        s.SetAngle(s.ZoneStart + s.ZoneSize / 2f);
        Assert.IsTrue(s.Stir());
        Assert.AreEqual(1, s.Hits);
        Assert.AreEqual(StirTimingLogic.Result.Hit, s.Last);
    }

    [Test]
    public void Stir_MissOutsideZone()
    {
        var s = new StirTimingLogic(1);
        s.SetAngle(s.ZoneStart + s.ZoneSize + 20f);
        Assert.IsFalse(s.Stir());
        Assert.AreEqual(1, s.Misses);
    }

    [Test]
    public void Stir_GetsHarderAfterAHit()
    {
        var s = new StirTimingLogic(1);
        float speed = s.Speed, zone = s.ZoneSize;
        s.SetAngle(s.ZoneStart + 1f);
        s.Stir();
        Assert.Greater(s.Speed, speed);
        Assert.Less(s.ZoneSize, zone);
    }

    [Test]
    public void Stir_EndsAfterAllStirs()
    {
        var s = new StirTimingLogic(1);
        for (int i = 0; i < s.stirsNeeded; i++) { s.SetAngle(s.ZoneStart + 1f); s.Stir(); }
        Assert.IsTrue(s.Done);
        Assert.AreEqual(1f, s.Score);
    }

    [Test]
    public void Stir_ZoneWrapsAround360()
    {
        var s = new StirTimingLogic(1);
        s.SetAngle(s.ZoneStart + s.ZoneSize - 1f + 360f);
        Assert.IsTrue(s.InZone);
    }

    [Test]
    public void Stir_BurnUndoesAGoodStir()
    {
        var s = new StirTimingLogic(1);
        s.SetAngle(s.ZoneStart + 1f);
        s.Stir();
        s.SetAngle(s.BurnStart + 1f);
        Assert.IsTrue(s.InBurnZone);
        Assert.IsFalse(s.Stir());
        Assert.AreEqual(StirTimingLogic.Result.Burn, s.Last);
        Assert.AreEqual(1, s.Burns);
        Assert.AreEqual(1, s.Misses);
        Assert.AreEqual(0f, s.Score, 1e-4);
    }

    [Test]
    public void Stir_BurnZoneNeverTouchesTheGreenZone()
    {
        for (int seed = 1; seed <= 40; seed++)
        {
            var s = new StirTimingLogic(seed);
            for (int i = 0; i < s.stirsNeeded; i++)
            {
                float burnAfterGreen = Mathf.Repeat(s.BurnStart - s.ZoneStart, 360f);
                float greenAfterBurn = Mathf.Repeat(s.ZoneStart - s.BurnStart, 360f);
                Assert.GreaterOrEqual(burnAfterGreen, s.ZoneSize, $"seed {seed} stir {i}");
                Assert.GreaterOrEqual(greenAfterBurn, s.BurnSize, $"seed {seed} stir {i}");
                s.SetAngle(s.ZoneStart + 1f);
                s.Stir();
            }
        }
    }

    [Test]
    public void Grind_AlternatingPressesCount()
    {
        var g = new GrindLogic();
        Assert.IsTrue(g.Press(GrindLogic.Side.Left));
        Assert.IsTrue(g.Press(GrindLogic.Side.Right));
        Assert.AreEqual(2, g.Strokes);
    }

    [Test]
    public void Grind_SameSideTwiceDoesNotCount()
    {
        var g = new GrindLogic();
        g.Press(GrindLogic.Side.Left);
        Assert.IsFalse(g.Press(GrindLogic.Side.Left));
        Assert.AreEqual(1, g.Strokes);
    }

    [Test]
    public void Grind_RunsOutOfTime()
    {
        var g = new GrindLogic();
        g.Press(GrindLogic.Side.Left);
        g.Tick(g.timeLimit + 1f);
        Assert.IsTrue(g.Done);
        Assert.IsFalse(g.Succeeded);
        Assert.IsFalse(g.Press(GrindLogic.Side.Right));
        Assert.AreEqual(1f / g.strokesNeeded, g.Score, 1e-4);
    }

    [Test]
    public void Grind_FinishesAfterEnoughStrokes()
    {
        var g = new GrindLogic();
        for (int i = 0; i < g.strokesNeeded; i++) g.Press(i % 2 == 0 ? GrindLogic.Side.Left : GrindLogic.Side.Right);
        Assert.IsTrue(g.Done);
        Assert.IsTrue(g.Succeeded);
        Assert.AreEqual(1f, g.Score, 1e-4);
    }

    [Test]
    public void Craft_CombineUsesTheAverageInputQuality()
    {
        Assert.AreEqual(0.4f, CraftQuality.Combine(0.8f, 1f, 0f), 1e-4);
        Assert.AreEqual(0.5f, CraftQuality.Combine(0.5f), 1e-4);
        Assert.AreEqual(1f, CraftQuality.Combine(2f, 1f), 1e-4);
    }

    [Test]
    public void Craft_MultiplierKeepsTheUnweightedPart()
    {
        Assert.AreEqual(1f, CraftQuality.Multiplier(0f, 0f), 1e-4);
        Assert.AreEqual(0.6f, CraftQuality.Multiplier(0f, 0.4f), 1e-4);
        Assert.AreEqual(1f, CraftQuality.Multiplier(1f, 0.4f), 1e-4);
        Assert.AreEqual(0.8f, CraftQuality.Multiplier(0.5f, 0.4f), 1e-4);
    }

    // ---- brew (Aliyah's cauldron)

    private static CauldronBrewLogic.Need SlowCounter(int turns = 1) => new CauldronBrewLogic.Need
        { name = "a", direction = CauldronBrewLogic.Direction.Counterclockwise, fast = false, turns = turns };

    // keeps the fire in range while ticking, so only the stirring is under test
    private static void Run(CauldronBrewLogic b, float seconds, float stir)
    {
        for (int i = 0; i < seconds * 60 && !b.Done; i++) b.Tick(1f / 60f, stir, b.Heat < b.heatMin + 0.05f);
    }

    [Test]
    public void Brew_StirringTheRightWayCompletesTheIngredient()
    {
        var b = new CauldronBrewLogic(new[] { SlowCounter() });
        Run(b, 8f, 0.3f); // 0.3 of the stick is 126 deg/s, inside the slow band
        Assert.IsTrue(b.Done);
        Assert.AreEqual(1, b.Current);
        Assert.Greater(b.Score, 0.95f);
    }

    [Test]
    public void Brew_WrongDirectionDoesNotCount()
    {
        var b = new CauldronBrewLogic(new[] { SlowCounter() });
        Run(b, 4f, -0.3f);
        Assert.AreEqual(0f, b.Needs[0].Done, 1e-4);
        Assert.AreEqual(0, b.Current);
    }

    [Test]
    public void Brew_TooFastDoesNotCount()
    {
        var b = new CauldronBrewLogic(new[] { SlowCounter() });
        Run(b, 4f, 1f); // full push is 420 deg/s, way over the slow band; only the ramp through it counts
        Assert.Less(b.Needs[0].Done, 0.1f);
        Assert.AreEqual(0, b.Current);
        Assert.IsFalse(b.SpeedInBand(b.Needs[0]));
    }

    [Test]
    public void Brew_FireDiesDownAndTheBellowsBringItBack()
    {
        var b = new CauldronBrewLogic(new[] { SlowCounter(3) });
        float start = b.Heat;
        for (int i = 0; i < 60 * 3; i++) b.Tick(1f / 60f, 0f, false);
        Assert.Less(b.Heat, start);
        float low = b.Heat;
        b.Tick(1f / 60f, 0f, true);
        Assert.AreEqual(low + b.stokeAmount - b.heatDecay / 60f, b.Heat, 1e-3);
    }

    [Test]
    public void Brew_StirringDoesNotCountWhileTheFireIsLow()
    {
        var b = new CauldronBrewLogic(new[] { SlowCounter(3) });
        for (int i = 0; i < 60 * 10; i++) b.Tick(1f / 60f, 0f, false);
        Assert.IsTrue(b.FireTooLow);
        float before = b.Needs[0].Done;
        for (int i = 0; i < 60 * 2; i++) b.Tick(1f / 60f, 0.3f, false);
        Assert.AreEqual(before, b.Needs[0].Done, 1e-4);
    }

    [Test]
    public void Brew_OverheatingScorchesAndCostsScore()
    {
        var b = new CauldronBrewLogic(new[] { SlowCounter(3) });
        for (int i = 0; i < 60 * 3; i++) b.Tick(1f / 60f, 0.3f, true); // pumping every frame
        Assert.IsTrue(b.FireTooHot);
        Assert.Greater(b.Scorch, 0f);
        Assert.AreEqual(b.Progress * (1f - 0.5f * b.Scorch), b.Score, 1e-4);
    }

    [Test]
    public void Brew_RunsOutOfTime()
    {
        var b = new CauldronBrewLogic(new[] { SlowCounter(3) }) { timeLimit = 2f };
        for (int i = 0; i < 60 * 3; i++) b.Tick(1f / 60f, 0f, false);
        Assert.IsTrue(b.Done);
        Assert.AreEqual(0, b.Current);
    }

    [Test]
    public void Brew_FlamefliesMakeTheBellowsStronger()
    {
        var plain = new CauldronBrewLogic(new[] { SlowCounter() });
        var fuelled = new CauldronBrewLogic(new[] { SlowCounter() }, 5);
        Assert.Greater(fuelled.stokeAmount, plain.stokeAmount);
        Assert.GreaterOrEqual(fuelled.Heat, plain.Heat);
    }

    [Test]
    public void Brew_NeedIsStableForAnIngredientAndHarderWhenPoorlyRefined()
    {
        var a = CauldronBrewLogic.NeedFor("Heartroot", Color.red, 1f);
        var b = CauldronBrewLogic.NeedFor("Heartroot", Color.red, 1f);
        Assert.AreEqual(a.direction, b.direction);
        Assert.AreEqual(a.fast, b.fast);
        Assert.AreEqual(2, a.turns);
        Assert.AreEqual(3, CauldronBrewLogic.NeedFor("Heartroot", Color.red, 0.4f).turns);
    }

    // ---- chop to the beat

    [Test]
    public void Rhythm_PressOnTheBeatIsPerfect()
    {
        var r = new RhythmChopLogic(2) { bpm = 120f };
        r.Tick(r.CutTime(0));
        Assert.AreEqual(RhythmChopLogic.Judgement.Perfect, r.Press());
        Assert.AreEqual(1, r.Perfects);
    }

    [Test]
    public void Rhythm_PressALittleLateIsGood()
    {
        var r = new RhythmChopLogic(2) { bpm = 120f };
        r.Tick(r.CutTime(0) + 0.12f);
        Assert.AreEqual(RhythmChopLogic.Judgement.Good, r.Press());
    }

    [Test]
    public void Rhythm_TooEarlyDoesNotCount()
    {
        var r = new RhythmChopLogic(2) { bpm = 120f };
        r.Tick(r.CutTime(0) - 0.5f);
        Assert.AreEqual(RhythmChopLogic.Judgement.None, r.Press());
        Assert.AreEqual(0, r.NextCut);
    }

    [Test]
    public void Rhythm_LettingTheLineSlidePastIsAMiss()
    {
        var r = new RhythmChopLogic(2) { bpm = 120f };
        r.Tick(r.CutTime(0) + r.goodWindow + 0.01f);
        Assert.AreEqual(1, r.Misses);
        Assert.AreEqual(1, r.NextCut);
    }

    [Test]
    public void Rhythm_ScoreWeighsGoodsBelowPerfects()
    {
        var r = new RhythmChopLogic(2) { bpm = 120f };
        r.Tick(r.CutTime(0)); r.Press();
        r.Tick(r.CutTime(1) - r.Time + 0.12f); r.Press();
        Assert.IsTrue(r.Done);
        Assert.AreEqual(0.8f, r.Score, 1e-4);
    }
}
