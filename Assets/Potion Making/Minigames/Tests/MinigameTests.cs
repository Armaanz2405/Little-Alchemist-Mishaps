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
}
