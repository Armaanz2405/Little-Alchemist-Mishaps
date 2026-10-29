using NUnit.Framework;
using Nocturne.PotionMaking.Minigames;
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
}
