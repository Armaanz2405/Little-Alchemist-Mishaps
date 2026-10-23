using NUnit.Framework;
using Nocturne.PotionMaking.Energy;
using UnityEngine;

public class EnergyTests
{
    EnergyMeter meter;
    GameObject go;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("Meter");
        meter = go.AddComponent<EnergyMeter>();
        meter.Refill(6);
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(go);

    [Test]
    public void Spending_LowersEnergy()
    {
        Assert.IsTrue(meter.TrySpend(2));
        Assert.AreEqual(4, meter.Current);
    }

    [Test]
    public void CantSpendMoreThanYouHave()
    {
        Assert.IsFalse(meter.TrySpend(7));
        Assert.AreEqual(6, meter.Current);
    }

    [Test]
    public void HittingZero_FiresOutOfEnergy()
    {
        bool fired = false;
        meter.onOutOfEnergy.AddListener(() => fired = true);
        meter.TrySpend(6);
        Assert.IsTrue(fired);
        Assert.IsFalse(meter.TrySpend(1));
    }

    [Test]
    public void Refill_SetsNewMax()
    {
        meter.TrySpend(3);
        meter.Refill(8);
        Assert.AreEqual(8, meter.Current);
        Assert.AreEqual(8, meter.Max);
    }

    [Test]
    public void Costs_AreLookedUpPerAction()
    {
        var costs = ScriptableObject.CreateInstance<EnergyCosts>();
        costs.inspect = 1; costs.refine = 2; costs.bottle = 0; costs.talk = 1;
        Assert.AreEqual(2, costs.CostOf(EnergyAction.Refine));
        Assert.AreEqual(0, costs.CostOf(EnergyAction.Bottle));
    }

    [Test]
    public void KeyNpcs_GetTheirOwnAmount_RegularsGetStandard()
    {
        var customers = ScriptableObject.CreateInstance<CustomerEnergy>();
        customers.standardEnergy = 5;
        customers.keyNpcs.Add(new CustomerEnergy.KeyNpc { npcName = "Librarian", energy = 8 });
        Assert.AreEqual(8, customers.EnergyFor("Librarian"));
        Assert.AreEqual(5, customers.EnergyFor("Joe Schmo"));
    }
}
