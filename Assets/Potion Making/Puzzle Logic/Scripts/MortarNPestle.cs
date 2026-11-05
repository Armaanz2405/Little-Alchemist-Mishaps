using System.Collections.Generic;
using System.Linq;
using Nocturne.PotionMaking.Energy;
using Nocturne.PotionMaking.Minigames;
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
    [Tooltip("Optional. When set, a full mortar waits for the grind minigame instead of packing on its own.")]
    [SerializeField] private MinigameStation grindMinigameStation;
    public UnityEvent<IngredientData> onIngredientsCrushed = new UnityEvent<IngredientData>();

    private readonly List<IngredientData> contents = new List<IngredientData>();
    public IReadOnlyList<IngredientData> Contents => contents;
    public bool IsFull => contents.Count >= MaxIngredientsPerCrush;
    public bool HasGrindMinigame => grindMinigameStation != null;
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
        if (IsFull) return;
        if (contents.Count == MaxIngredientsPerCrush - 1 && energy != null && !energy.CanAfford(EnergyAction.Refine)) return;
        if (!ingredient.TryUse()) return;
        AddIngredient(ingredient.data);
        Destroy(ingredient.gameObject);
    }

    public IngredientData AddIngredient(IngredientData ingredient)
    {
        contents.Add(ingredient);
        Debug.Log($"[Mortar] Added {ingredient.displayName} ({contents.Count}/{MaxIngredientsPerCrush})");
        fillAmntText.text = $"({contents.Count}/{MaxIngredientsPerCrush})";
        //onIngredientAdded.Invoke(ingredient);

        if (!IsFull) return null;
        if (grindMinigameStation != null)
        {
            grindMinigameStation.isReadyToPlay = true;
            return null;
        }
        return Pack();
    }

    public void Grind_Trigger()
    {
        if (IsFull) Pack();
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
        float grind = grindMinigameStation != null && grindMinigameStation.Game != null ? grindMinigameStation.Game.LastScore : 1f;
        packedIngredients.refineQuality = CraftQuality.Combine(grind, contents.Select(c => c.refineQuality));
        contents.Clear();
        fillAmntText.text = $"({contents.Count}/{MaxIngredientsPerCrush})";

        Debug.Log($"[Mortar] Ground {packedIngredients.displayName}, quality {Mathf.RoundToInt(packedIngredients.refineQuality * 100)}%" + "\n" + ListContents(packedIngredients.ingredientEffects));

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
