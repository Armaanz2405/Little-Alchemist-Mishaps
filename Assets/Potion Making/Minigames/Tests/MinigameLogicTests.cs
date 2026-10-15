using NUnit.Framework;
using Nocturne.PotionMaking.Minigames;
using UnityEngine;

public class MinigameLogicTests
{
    const float Dt = 1f / 60f;

    static void Stir(StirLogic s, float turnsPerSec, float seconds)
    {
        for (float t = 0; t < seconds && !s.Done; t += Dt) s.Tick(Dt, turnsPerSec * 2f * Mathf.PI * Dt);
    }

    [Test]
    public void Stir_SlowWhenCalm_FastWhenBubbling_Succeeds()
    {
        var s = new StirLogic();
        for (int i = 0; i < 2000 && !s.Done; i++) s.Tick(Dt, (s.Bubbling ? 2.2f : 0.8f) * 2f * Mathf.PI * Dt);
        Assert.IsTrue(s.Succeeded);
        Assert.Greater(s.Score, 0.8f);
    }

    [Test]
    public void Stir_TooSlowWhileBubbling_BoilsOver()
    {
        var s = new StirLogic();
        for (int i = 0; i < 4000 && !s.Done; i++) s.Tick(Dt, 0.8f * 2f * Mathf.PI * Dt);
        Assert.IsFalse(s.Succeeded);
        Assert.AreEqual(1f, s.Overflow, 1e-4);
        Assert.AreEqual(0f, s.Score);
    }

    [Test]
    public void Stir_TooFastWhenCalm_Splashes()
    {
        var s = new StirLogic();
        Stir(s, 3f, 1f);
        Assert.Less(s.Progress, 0.1f);
        Assert.Greater(s.Overflow, 0.3f);
    }

    [Test]
    public void Stir_NotStirringWhileCalm_IsSafe()
    {
        var s = new StirLogic();
        for (int i = 0; i < 120; i++) s.Tick(Dt, 0f);
        Assert.AreEqual(0f, s.Overflow);
    }

    [Test]
    public void Chop_CountsCleanCuts()
    {
        var c = new ChopLogic();
        c.SetKnife(c.CurrentTarget); Assert.IsTrue(c.Chop());
        c.SetKnife(0f);              Assert.IsFalse(c.Chop());
        c.SetKnife(c.CurrentTarget); Assert.IsTrue(c.Chop());
        Assert.IsTrue(c.Done);
        Assert.AreEqual(2, c.Hits);
        Assert.AreEqual(2f / 3f, c.Score, 1e-4);
    }

    [Test]
    public void Chop_GetsFasterEachCut()
    {
        var c = new ChopLogic();
        float first = c.Speed;
        c.Chop();
        Assert.Greater(c.Speed, first);
    }

    [Test]
    public void Grind_OnlyAlternatingStrokesCount()
    {
        var g = new GrindLogic { strokesNeeded = 4 };
        g.Reset();
        Assert.IsTrue(g.Press(GrindLogic.Side.Left));
        Assert.IsFalse(g.Press(GrindLogic.Side.Left));
        Assert.IsTrue(g.Press(GrindLogic.Side.Right));
        Assert.IsTrue(g.Press(GrindLogic.Side.Left));
        Assert.IsTrue(g.Press(GrindLogic.Side.Right));
        Assert.IsTrue(g.Succeeded);
    }

    [Test]
    public void Grind_TimerRunningOut_GivesPartialScore()
    {
        var g = new GrindLogic { strokesNeeded = 10, timeLimit = 1f };
        g.Reset();
        g.Press(GrindLogic.Side.Left); g.Press(GrindLogic.Side.Right);
        g.Tick(2f);
        Assert.IsTrue(g.Done);
        Assert.IsFalse(g.Succeeded);
        Assert.AreEqual(0.2f, g.Score, 1e-4);
    }

    [Test]
    public void Pour_ReleaseInBand_IsPerfect()
    {
        var p = new PourLogic();
        float holdFor = (p.targetMin + p.targetMax) / 2f / p.pourRate;
        for (float t = 0; t < holdFor; t += Dt) p.Tick(Dt, true);
        p.Tick(Dt, false);
        Assert.IsTrue(p.Done);
        Assert.IsTrue(p.InTarget);
        Assert.AreEqual(1f, p.Score);
    }

    [Test]
    public void Pour_HoldingTooLong_Spills()
    {
        var p = new PourLogic();
        for (int i = 0; i < 600 && !p.Done; i++) p.Tick(Dt, true);
        Assert.IsTrue(p.Spilled);
        Assert.AreEqual(0f, p.Score);
    }

    [Test]
    public void Pour_NothingHappensUntilYouStartPouring()
    {
        var p = new PourLogic();
        p.Tick(Dt, false);
        Assert.IsFalse(p.Done);
    }
}
