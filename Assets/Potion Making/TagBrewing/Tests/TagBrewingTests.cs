using System.Collections.Generic;
using NUnit.Framework;
using Nocturne.PotionMaking.TagBrewing;
using UnityEngine;
using E = IngredientTags.Effects;

public class TagBrewingTests
{
    TagRules rules;

    static IngredientData Make(string name, params (E effect, int value)[] tags)
    {
        var i = ScriptableObject.CreateInstance<IngredientData>();
        i.displayName = name;
        foreach (var t in tags) i.ingredientEffects.Add(new IngredientTags { tag_effect = t.effect, tag_value = t.value });
        return i;
    }

    [SetUp]
    public void SetUp()
    {
        rules = ScriptableObject.CreateInstance<TagRules>();
        rules.opposing.Add(new TagRules.OpposingPair { a = E.SLEEPY, b = E.STRENGTH });
        rules.safeIngredientCount = 4;
        rules.overflowChancePerExtra = 0.25f;
    }

    [Test]
    public void TagsAddUp()
    {
        var tags = TagMath.Combine(new[] { Make("A", (E.BUBBLY, 2)), Make("B", (E.BUBBLY, 1), (E.POISON, 1)) }, rules);
        Assert.AreEqual(3, tags[E.BUBBLY]);
        Assert.AreEqual(1, tags[E.POISON]);
    }

    [Test]
    public void OpposingTagsCancelPointForPoint()
    {
        var tags = TagMath.Combine(new[] { Make("Sleepy", (E.SLEEPY, 3)), Make("Strong", (E.STRENGTH, 2)) }, rules);
        Assert.AreEqual(1, tags[E.SLEEPY]);
        Assert.IsFalse(tags.ContainsKey(E.STRENGTH));
    }

    [Test]
    public void ZeroValueTagsAreIgnored()
    {
        var tags = TagMath.Combine(new[] { Make("Fern", (E.SLEEPY, 2), (E.POISON, 0)) }, rules);
        Assert.AreEqual(1, tags.Count);
    }

    [Test]
    public void DominantTag_IsTheBiggest()
    {
        var tags = new Dictionary<E, int> { { E.POISON, 1 }, { E.BUBBLY, 3 } };
        Assert.AreEqual(E.BUBBLY, TagMath.Dominant(tags));
        Assert.AreEqual("Bubbly 3  ·  Poison 1", TagMath.Describe(tags));
    }

    [Test]
    public void OverflowRisk_StartsAfterTheSafeCount()
    {
        Assert.AreEqual(0f, TagMath.OverflowChance(4, rules));
        Assert.AreEqual(0.25f, TagMath.OverflowChance(5, rules), 1e-5);
        Assert.AreEqual(1f, TagMath.OverflowChance(9, rules), 1e-5);
    }

    [Test]
    public void Cauldron_BottlesDominantTagPotion()
    {
        var go = new GameObject("Cauldron", typeof(BoxCollider2D));
        var cauldron = go.AddComponent<TagCauldron>();
        typeof(TagCauldron).GetField("rules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cauldron, rules);
        cauldron.roll = () => 0.99f;
        cauldron.AddIngredient(Make("A", (E.BUBBLY, 2)));
        cauldron.AddIngredient(Make("B", (E.SLEEPY, 1)));
        var potion = cauldron.Bottle();
        Assert.AreEqual("Bubbly Potion", potion.displayName);
        Assert.AreEqual(0, cauldron.Contents.Count);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Cauldron_CanBubbleOverPastSafeCount()
    {
        var go = new GameObject("Cauldron", typeof(BoxCollider2D));
        var cauldron = go.AddComponent<TagCauldron>();
        typeof(TagCauldron).GetField("rules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(cauldron, rules);
        int lost = -1;
        cauldron.onBubbledOver.AddListener(n => lost = n);
        cauldron.roll = () => 0.1f;
        for (int i = 0; i < 4; i++) Assert.IsTrue(cauldron.AddIngredient(Make("X", (E.BUBBLY, 1))));
        Assert.IsFalse(cauldron.AddIngredient(Make("X", (E.BUBBLY, 1))));
        Assert.AreEqual(5, lost);
        Assert.AreEqual(0, cauldron.Contents.Count);
        Object.DestroyImmediate(go);
    }
}
