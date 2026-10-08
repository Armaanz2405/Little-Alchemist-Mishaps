using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PotionRecipeTests
{
    IngredientData bubblecap, featherfern, frostcap;
    PotionData springstep, sludge;
    PotionRecipeBook book;

    [SetUp]
    public void SetUp()
    {
        bubblecap = Ingredient("Bubblecap");
        featherfern = Ingredient("Featherfern");
        frostcap = Ingredient("Frostcap");
        springstep = ScriptableObject.CreateInstance<PotionData>(); springstep.displayName = "Springstep Draught";
        sludge = ScriptableObject.CreateInstance<PotionData>(); sludge.displayName = "Murky Sludge";
        book = ScriptableObject.CreateInstance<PotionRecipeBook>();
        book.recipes.Add(new PotionRecipeBook.Recipe { first = bubblecap, second = featherfern, result = springstep });
        book.failedPotion = sludge;
    }

    static IngredientData Ingredient(string name)
    {
        var i = ScriptableObject.CreateInstance<IngredientData>();
        i.displayName = name;
        return i;
    }

    [Test]
    public void MatchingPair_MakesThePotion()
    {
        Assert.AreSame(springstep, book.Combine(bubblecap, featherfern));
    }

    [Test]
    public void OrderDoesNotMatter()
    {
        Assert.AreSame(springstep, book.Combine(featherfern, bubblecap));
    }

    [Test]
    public void UnknownPair_MakesTheFailedPotion()
    {
        Assert.AreSame(sludge, book.Combine(bubblecap, frostcap));
        Assert.AreSame(sludge, book.Combine(bubblecap, bubblecap));
    }

    [Test]
    public void RecipeWithoutResult_IsSkipped()
    {
        book.recipes.Insert(0, new PotionRecipeBook.Recipe { first = bubblecap, second = featherfern, result = null });
        Assert.AreSame(springstep, book.Combine(bubblecap, featherfern));
    }

    [Test]
    public void Cauldron_BrewsOnSecondIngredient_ThenEmpties()
    {
        var go = new GameObject("Cauldron", typeof(BoxCollider2D));
        var cauldron = go.AddComponent<Cauldron>();
        var so = new SerializedObject(cauldron);
        so.FindProperty("recipeBook").objectReferenceValue = book;
        so.ApplyModifiedPropertiesWithoutUndo();

        PotionData brewed = null;
        cauldron.onPotionBrewed.AddListener(p => brewed = p);

        Assert.IsNull(cauldron.AddIngredient(bubblecap), "one ingredient isn't a potion yet");
        Assert.AreEqual(1, cauldron.Contents.Count);
        Assert.AreSame(springstep, cauldron.AddIngredient(featherfern));
        Assert.AreSame(springstep, brewed, "onPotionBrewed should fire");
        Assert.AreEqual(0, cauldron.Contents.Count, "cauldron empties after brewing");

        Object.DestroyImmediate(go);
    }
}
