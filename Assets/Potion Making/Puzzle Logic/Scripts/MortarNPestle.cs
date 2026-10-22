using System.Collections.Generic;
using System.Linq;
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

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void Start()
    {
        fillAmntText.text = $"({contents.Count}/{MaxIngredientsPerCrush})";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var ingredient = other.GetComponentInParent<Ingredient>();
        if (ingredient == null || ingredient.data == null || !ingredient.TryUse()) return;
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

        var all_tags = contents[0].ingredientEffects.Concat(contents[1].ingredientEffects).Concat(contents[2].ingredientEffects);
        contents.Clear();
        fillAmntText.text = $"({contents.Count}/{MaxIngredientsPerCrush})";

        packedIngredients.displayName = "New Goop";
        packedIngredients.ingredientEffects = all_tags.ToList();
        packedIngredients.color = Color.black;

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
