using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// Drop two ingredients in and it brews a potion: ingredient 1 + ingredient 2 = potion.
// The collider must be a trigger. Held items have their Rigidbody2D un-simulated by PlayerMovement,
// so ingredients only count once they're dropped (or released) inside the cauldron.
[RequireComponent(typeof(Collider2D))]
public class PuzzleLogic_Cauldron : MonoBehaviour
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
        if (potionContentsText != null) potionContentsText.text = "";
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
        if (potionContentsText != null) potionContentsText.text = cauldronUI;


        //return contents.Count >= IngredientsPerPotion ? Brew() : null;

        if (contents.Count >= IngredientsPerPotion && stirMinigameStation != null) stirMinigameStation.isReadyToPlay = true;
        return null;
    }

    public void Brew_Trigger()
    {
        Brew();
    }

    private PotionData Brew()
    {
        if (contents.Count < IngredientsPerPotion) return null;
        PotionData result = MakePotion(contents[0], contents[1]);
        // how well it was stirred, times how well the ingredients were refined, becomes the potion's craft quality
        if (result != null && (recipeBook == null || result != recipeBook.failedPotion))
        {
            float stir = stirMinigameStation != null && stirMinigameStation.Game != null ? stirMinigameStation.Game.LastScore : 1f;
            result.craftQuality = Nocturne.PotionMaking.Minigames.CraftQuality.Combine(stir, contents.Select(c => c.refineQuality));
        }
        contents.Clear();
        contentsEffects.Clear();
        if (potionContentsText != null) potionContentsText.text = "";
        
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

    private PotionData MakePotion(IngredientData a, IngredientData b)
    {
        var totals = new Dictionary<IngredientTags.Effects, int>();
        foreach (var tag in a.ingredientEffects.Concat(b.ingredientEffects))
        {
            if (tag == null || tag.tag_value <= 0) continue;
            totals.TryGetValue(tag.tag_effect, out int v);
            totals[tag.tag_effect] = v + tag.tag_value;
        }
        if (totals.Count == 0) return recipeBook != null ? recipeBook.failedPotion : null;

        var strongest = totals.OrderByDescending(kv => kv.Value).ThenBy(kv => (int)kv.Key).First().Key;
        PotionData recipe = recipeBook != null ? recipeBook.Match(a, b) : null;
        PotionData potion = ScriptableObject.CreateInstance<PotionData>();
        potion.displayName = recipe != null ? recipe.displayName : $"{IngredientTags.Name(strongest)} Draught";
        potion.color = recipe != null ? recipe.color : IngredientTags.ColorOf(strongest);
        potion.sprite = recipe != null ? recipe.sprite : null;
        potion.potionEffects = totals.OrderByDescending(kv => kv.Value).ThenBy(kv => (int)kv.Key)
            .Select(kv => new IngredientTags { tag_effect = kv.Key, tag_value = kv.Value }).ToList();
        return potion;
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
            for (int i = 0; i < Mathf.Max(1, item.tag_value); i++) effects.Add(item.tag_effect);
        }
        return effects;
    }
}
