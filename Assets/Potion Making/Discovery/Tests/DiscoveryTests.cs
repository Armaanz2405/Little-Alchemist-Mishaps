using NUnit.Framework;
using Nocturne.PotionMaking.Discovery;
using UnityEngine;

public class DiscoveryTests
{
    GameObject go;
    DiscoveryLog log;
    IngredientData bubblecap, featherfern;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("Log");
        log = go.AddComponent<DiscoveryLog>();
        bubblecap = ScriptableObject.CreateInstance<IngredientData>(); bubblecap.displayName = "Bubblecap";
        bubblecap.ingredientEffects.Add(new IngredientTags { tag_effect = IngredientTags.Effects.BUBBLY, tag_value = 2 });
        featherfern = ScriptableObject.CreateInstance<IngredientData>(); featherfern.displayName = "Featherfern";
        featherfern.ingredientEffects.Add(new IngredientTags { tag_effect = IngredientTags.Effects.SLEEPY, tag_value = 2 });
        featherfern.ingredientEffects.Add(new IngredientTags { tag_effect = IngredientTags.Effects.POISON, tag_value = 0 });
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(go);

    [Test]
    public void IngredientsStartHidden()
    {
        Assert.IsFalse(log.IsDiscovered(bubblecap));
    }

    [Test]
    public void Discover_OnlyCountsTheFirstTime()
    {
        int events = 0;
        log.onDiscovered.AddListener(_ => events++);
        Assert.IsTrue(log.Discover(bubblecap));
        Assert.IsFalse(log.Discover(bubblecap));
        Assert.IsTrue(log.IsDiscovered(bubblecap));
        Assert.AreEqual(1, events);
        Assert.AreEqual(1, log.Count);
    }

    [Test]
    public void Catalog_GivesHintForHiddenEntries()
    {
        var catalog = ScriptableObject.CreateInstance<IngredientCatalog>();
        catalog.entries.Add(new IngredientCatalog.Entry { ingredient = bubblecap, hint = "Fizzes when squeezed." });
        Assert.AreEqual("Fizzes when squeezed.", catalog.HintFor(bubblecap));
        Assert.AreEqual("", catalog.HintFor(featherfern));
    }

    [Test]
    public void Tags_AreDescribedWithoutZeroValues()
    {
        Assert.AreEqual("Bubbly 2", IngredientCatalog.DescribeTags(bubblecap));
        Assert.AreEqual("Sleepy 2", IngredientCatalog.DescribeTags(featherfern));
    }
}
