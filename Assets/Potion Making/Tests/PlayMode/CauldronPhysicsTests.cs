using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Checks the cauldron with real 2D physics, the same way PlayerMovement drops items.
public class CauldronPhysicsTests
{
    IngredientData bubblecap, featherfern;
    PotionData springstep;
    Cauldron cauldron;
    Potion potionTemplate;
    readonly List<Object> created = new List<Object>();   // only clean up what this test made (not the test runner's objects)

    [SetUp]
    public void SetUp()
    {
        bubblecap = ScriptableObject.CreateInstance<IngredientData>(); bubblecap.displayName = "Bubblecap";
        featherfern = ScriptableObject.CreateInstance<IngredientData>(); featherfern.displayName = "Featherfern";
        springstep = ScriptableObject.CreateInstance<PotionData>(); springstep.displayName = "Springstep Draught";
        var book = ScriptableObject.CreateInstance<PotionRecipeBook>();
        book.recipes.Add(new PotionRecipeBook.Recipe { first = bubblecap, second = featherfern, result = springstep });

        var potionGo = new GameObject("PotionTemplate", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
        created.Add(potionGo);
        potionTemplate = potionGo.AddComponent<Potion>();
        potionGo.GetComponent<Rigidbody2D>().simulated = false;   // template only
        potionGo.transform.position = new Vector3(100, 100, 0);

        var go = new GameObject("Cauldron");
        created.Add(go);
        var box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.4f, 1f);
        cauldron = go.AddComponent<Cauldron>();
        Set(cauldron, "recipeBook", book);
        Set(cauldron, "potionPrefab", potionTemplate);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var p in Object.FindObjectsByType<Potion>()) created.Add(p.gameObject);
        foreach (var i in Object.FindObjectsByType<Ingredient>()) created.Add(i.gameObject);
        foreach (var o in created) if (o != null) Object.Destroy(o);
        created.Clear();
    }

    static void Set(object target, string field, object value) =>
        target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

    static Ingredient MakeIngredient(IngredientData data, Vector3 at)
    {
        var go = new GameObject(data.displayName, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
        go.transform.position = at;
        go.GetComponent<CircleCollider2D>().radius = 0.2f;
        var ingredient = go.AddComponent<Ingredient>();
        ingredient.data = data;
        return ingredient;
    }

    [UnityTest]
    public IEnumerator DroppingTwoIngredientsIn_BrewsAPotion()
    {
        MakeIngredient(bubblecap, new Vector3(-0.2f, 2f, 0));
        MakeIngredient(featherfern, new Vector3(0.2f, 3f, 0));
        yield return new WaitForSeconds(1.5f);

        Assert.AreEqual(0, Object.FindObjectsByType<Ingredient>().Length, "both ingredients should be used up");
        var potions = Object.FindObjectsByType<Potion>();
        Assert.AreEqual(2, potions.Length, "template + one brewed potion");
        bool brewed = false;
        foreach (var p in potions) if (p != potionTemplate && p.data == springstep) brewed = true;
        Assert.IsTrue(brewed, "a Springstep Draught should have popped out");
    }

    [UnityTest]
    public IEnumerator ReleasingAHeldIngredientInsideTheCauldron_Counts()
    {
        // PlayerMovement holds items with simulated = false and turns it back on when dropping.
        var held = MakeIngredient(bubblecap, Vector3.zero);
        var rb = held.GetComponent<Rigidbody2D>();
        rb.simulated = false;
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(0, cauldron.Contents.Count, "held items don't count");

        rb.simulated = true;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.AreEqual(1, cauldron.Contents.Count);
        Assert.AreSame(bubblecap, cauldron.Contents[0]);
    }
}
