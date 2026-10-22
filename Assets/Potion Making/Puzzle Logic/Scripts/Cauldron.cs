using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Drop two ingredients in and it brews a potion: ingredient 1 + ingredient 2 = potion.
// The collider must be a trigger. Held items have their Rigidbody2D un-simulated by PlayerMovement,
// so ingredients only count once they're dropped (or released) inside the cauldron.
[RequireComponent(typeof(Collider2D))]
public class Cauldron : MonoBehaviour
{
    public const int IngredientsPerPotion = 2;

    [SerializeField] private PotionRecipeBook recipeBook;
    [SerializeField] private Potion potionPrefab;
    [Tooltip("Where the finished potion pops out. Defaults to just above the cauldron.")]
    [SerializeField] private Transform potionSpawnPoint;
    [Tooltip("Optional: tinted with the latest ingredient so you can see what's inside.")]
    [SerializeField] private SpriteRenderer liquid;
    [SerializeField] private Color emptyLiquidColor = new Color(0.24f, 0.2f, 0.27f);
    [SerializeField] private TextMeshProUGUI potionContentsText;
    [SerializeField] private Nocturne.PotionMaking.Minigames.MinigameStation stirMinigameStation;

    // Hooks for sound/VFX/UI without touching this script.
    public UnityEvent<IngredientData> onIngredientAdded = new UnityEvent<IngredientData>();
    public UnityEvent<PotionData> onPotionBrewed = new UnityEvent<PotionData>();

    private readonly List<IngredientData> contents = new List<IngredientData>();
    private readonly List<IngredientTags.Effects> contentsEffects = new List<IngredientTags.Effects>();
    public IReadOnlyList<IngredientData> Contents => contents;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void Start()
    {
        if (liquid != null) liquid.color = emptyLiquidColor;
        potionContentsText.text = "";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ingredient = other.GetComponentInParent<Ingredient>();
        if (ingredient == null || ingredient.data == null || !ingredient.TryUse()) return;
        AddIngredient(ingredient.data);
        Destroy(ingredient.gameObject);
    }

    // Returns the brewed potion when this was the second ingredient, otherwise null.
    public PotionData AddIngredient(IngredientData ingredient)
    {
        contents.Add(ingredient);
        if (liquid != null) liquid.color = ingredient.color;
        Debug.Log($"[Cauldron] Added {ingredient.displayName} ({contents.Count}/{IngredientsPerPotion})");
        onIngredientAdded.Invoke(ingredient);

        //this is messy i know, i just wanted to test out getting the tags to combine together for a readable string
        contentsEffects.AddRange(ListContentsByEffects(ingredient.ingredientEffects));
        HashSet<IngredientTags.Effects> uniqueEffects = new HashSet<IngredientTags.Effects>(contentsEffects);

        string cauldronUI = "";
        foreach (var item in uniqueEffects)
        {
            cauldronUI += $"{item}:{CountTagsInBrew(contentsEffects, item)}\n";
        }
        potionContentsText.text = cauldronUI;


        //return contents.Count >= IngredientsPerPotion ? Brew() : null;

        if (contents.Count >= IngredientsPerPotion) stirMinigameStation.isReadyToPlay = true;
        return null;
    }

    public void Brew_Trigger()
    {
        Brew();
    }

    private PotionData Brew()
    {
        PotionData result = recipeBook != null ? recipeBook.Combine(contents[0], contents[1]) : null;
        contents.Clear();
        contentsEffects.Clear();
        
        if (liquid != null) liquid.color = emptyLiquidColor;

        if (result == null)
        {
            Debug.LogWarning("[Cauldron] No matching recipe and no failed potion set on the recipe book.");
            return null;
        }

        Debug.Log($"[Cauldron] Brewed {result.displayName}");
        if (potionPrefab != null)
        {
            Vector3 at = potionSpawnPoint != null ? potionSpawnPoint.position : transform.position + Vector3.up;
            Potion potion = Instantiate(potionPrefab, at, Quaternion.identity);
            potion.data = result;
            potion.ApplyLook();
        }
        onPotionBrewed.Invoke(result);
        return result;
    }

    private int CountTagsInBrew(List<IngredientTags.Effects> list, IngredientTags.Effects effect)
    {
        int count = 0;
        foreach (var tag in list)
        {
            count += (tag == effect) ? 1 : 0;
        }

        return count;
    }

    private List<IngredientTags.Effects> ListContentsByEffects(List<IngredientTags> l)
    {
        List<IngredientTags.Effects> effects = new List<IngredientTags.Effects>();
        foreach (var item in l)
        {
            effects.Add(item.tag_effect);
        }
        return effects;
    }
}
