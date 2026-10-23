using System.Collections.Generic;
using System.Linq;
using Nocturne.PotionMaking.Energy;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class MortarNPestle : MonoBehaviour
{
    public const int MaxIngredientsPerCrush = 3;

    [SerializeField] private Ingredient ingredientPrefab;
    [Tooltip("Where the packed ingredient pops out. Defaults to just above the mortar.")]
    [SerializeField] private Transform ingredientSpawnPoint;

    [SerializeField] private TextMeshProUGUI fillAmntText;
    public UnityEvent<IngredientData> onIngredientsCrushed = new UnityEvent<IngredientData>();

    private readonly List<IngredientData> contents = new List<IngredientData>();
    public IReadOnlyList<IngredientData> Contents => contents;
    private EnergyMeter energy;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void Start()
    {
        energy = FindAnyObjectByType<EnergyMeter>();
        fillAmntText.text = $"({contents.Count}/{MaxIngredientsPerCrush})";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ingredient = other.GetComponentInParent<Ingredient>();
        if (ingredient == null || ingredient.data == null) return;
        if (contents.Count == MaxIngredientsPerCrush - 1 && energy != null && !energy.CanAfford(EnergyAction.Refine)) return;
        if (!ingredient.TryUse()) return;
        AddIngredient(ingredient.data);
        Destroy(ingredient.gameObject);
    }

    public IngredientData AddIngredient(IngredientData ingredient)
    {
        contents.Add(ingredient);
        Debug.Log($"[Cauldron] Added {ingredient.displayName} ({contents.Count}/{MaxIngredientsPerCrush})");
        fillAmntText.text = $"({contents.Count}/{MaxIngredientsPerCrush})";
        //onIngredientAdded.Invoke(ingredient);

        // the minigame would open before this, giving the player a choice to crush or dump all ingredients out.
        return contents.Count >= MaxIngredientsPerCrush ? Pack() : null;
    }

    private IngredientData Pack()
    {
        IngredientData packedIngredients = ScriptableObject.CreateInstance<IngredientData>();

        var all_tags = contents.SelectMany(c => c.ingredientEffects)
            .Where(tag => tag != null && tag.tag_value > 0)
            .GroupBy(tag => tag.tag_effect)
            .Select(g => new IngredientTags { tag_effect = g.Key, tag_value = g.Sum(tag => tag.tag_value) })
            .OrderByDescending(tag => tag.tag_value).ThenBy(tag => (int)tag.tag_effect)
            .ToList();

        bool hasTags = all_tags.Count > 0;
        packedIngredients.displayName = hasTags ? $"{IngredientTags.Name(all_tags[0].tag_effect)} Paste" : "New Goop";
        packedIngredients.ingredientEffects = all_tags;
        packedIngredients.color = hasTags ? IngredientTags.ColorOf(all_tags[0].tag_effect) : Color.black;
        contents.Clear();
        fillAmntText.text = $"({contents.Count}/{MaxIngredientsPerCrush})";

        Debug.Log($"[Cauldron] Brewed {packedIngredients.displayName}" + "\n" + ListContents(packedIngredients.ingredientEffects));
        
        if (ingredientPrefab != null)
        {
            Vector3 at = ingredientSpawnPoint != null ? ingredientSpawnPoint.position : transform.position + Vector3.up;
            Ingredient ingredient = Instantiate(ingredientPrefab, at, Quaternion.identity);
            ingredient.data = packedIngredients;
            ingredient.ApplyLook();
        }

        onIngredientsCrushed.Invoke(packedIngredients);
        return packedIngredients;
    }

    private string ListContents(List<IngredientTags> l)
    {
        string result = "List contents: ";
        foreach (var item in l)
        {
            result += item.tag_effect.ToString() + ", ";
        }
        return result;
    }
}
