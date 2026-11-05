using System.Collections.Generic;
using Nocturne.PotionMaking.Discovery;
using Nocturne.PotionMaking.Energy;
using Nocturne.PotionMaking.Minigames;
using UnityEngine;
using Yarn.Unity;

// the first ordered tag is the main one, the rest are lesser tags
public class Customer_Order : MonoBehaviour
{
    public class Submission
    {
        public string potionName;
        public string potionTags;
        public string order;
        public int score;
        public bool passed;
        public float craftQuality;
        public int minigamesPlayed;
        public float minigameScoreTotal;
        public int energyLeft;
        public int energyMax;
    }

    public List<IngredientTags> ordered_tags = new List<IngredientTags>();
    [SerializeField] private VariableStorageBehaviour variableStorage;
    [SerializeField] private IngredientCatalog availableIngredients;
    [SerializeField] private CustomerEnergy customerEnergy;

    [Header("Grading")]
    [SerializeField] private int mainTagPoints = 40;
    [SerializeField] private int lesserTagPoints = 30;
    [SerializeField] private int sizePoints = 30;
    [SerializeField] private int extraTagPenalty = 15;
    [Tooltip("How much of the score the station minigames can take away. 0.4 means a botched craft keeps 60%.")]
    [Range(0f, 1f)] [SerializeField] private float craftWeight = 0.4f;
    [SerializeField] private int passScore = 60;

    private readonly List<Submission> submissions = new List<Submission>();
    private GrabAndThrow player;
    private EnergyMeter energy;
    private int minigamesPlayed;
    private float minigameScoreTotal;

    public IReadOnlyList<Submission> Submissions => submissions;
    public string OrderText => Describe(ordered_tags);
    public int MinigamesPlayed => minigamesPlayed;
    public float MinigameScoreTotal => minigameScoreTotal;

    void Start()
    {
        if (variableStorage == null) variableStorage = FindAnyObjectByType<VariableStorageBehaviour>();
        GameObject found = GameObject.FindWithTag("Player");
        if (found != null) player = found.GetComponent<GrabAndThrow>();
        energy = FindAnyObjectByType<EnergyMeter>();
        foreach (var minigame in FindObjectsByType<Minigame>(FindObjectsInactive.Include)) minigame.onFinished.AddListener(OnMinigameFinished);
        ResetEnergy();
        SetOrderVariables();
    }

    [YarnCommand("grade_potion")]
    public void GradePotion()
    {
        Potion potion = player != null && player.HeldItem != null ? player.HeldItem.GetComponent<Potion>() : null;
        if (potion == null || potion.data == null)
        {
            SetVariable("$isPotionPassable", false);
            return;
        }

        int score = Score(potion.data);
        submissions.Add(new Submission
        {
            potionName = potion.data.displayName,
            potionTags = DescribeValues(potion.data.potionEffects),
            order = OrderText,
            score = score,
            passed = score >= passScore,
            craftQuality = potion.data.craftQuality,
            minigamesPlayed = minigamesPlayed,
            minigameScoreTotal = minigameScoreTotal,
            energyLeft = energy != null ? energy.Current : 0,
            energyMax = energy != null ? energy.Max : 0,
        });
        Debug.Log($"[Customer] {potion.data.displayName} for {OrderText} scored {score} (craft {Mathf.RoundToInt(potion.data.craftQuality * 100)}%)");
        SetVariable("$potionScore", score);
        SetVariable("$isPotionPassable", score >= passScore);
        player.ConsumeHeldItem();
        NewOrder();
        ResetEnergy();
        minigamesPlayed = 0;
        minigameScoreTotal = 0f;
    }

    private void OnMinigameFinished(float score)
    {
        minigamesPlayed++;
        minigameScoreTotal += score;
    }

    public void ResetEnergy()
    {
        if (energy == null) return;
        energy.Refill(customerEnergy != null ? customerEnergy.standardEnergy : energy.Max);
    }



    // main tag, lesser tags, main one strongest, minus extra tags, then scaled by how well it was crafted
    public int Score(PotionData potion)
    {
        if (ordered_tags.Count == 0) return 0;
        int main = potion.ValueOf(ordered_tags[0].tag_effect);
        int lesserCount = ordered_tags.Count - 1;
        int score = main > 0 ? mainTagPoints : 0;
        bool sized = main > 0;
        if (lesserCount == 0 && main > 0) score += lesserTagPoints;
        for (int i = 1; i < ordered_tags.Count; i++)
        {
            int lesser = potion.ValueOf(ordered_tags[i].tag_effect);
            if (lesser > 0) score += lesserTagPoints / lesserCount;
            if (lesser <= 0 || lesser >= main) sized = false;
        }
        if (sized) score += sizePoints;
        foreach (var tag in potion.potionEffects)
            if (tag != null && !ordered_tags.Exists(t => t.tag_effect == tag.tag_effect)) score -= extraTagPenalty;
        score = Mathf.RoundToInt(score * CraftQuality.Multiplier(potion.craftQuality, craftWeight));
        return Mathf.Clamp(score, 0, 100);
    }

    // tag pairs from one ingredient so it can always be made
    private void NewOrder()
    {
        var pairs = new List<(IngredientTags.Effects main, IngredientTags.Effects lesser)>();
        foreach (var ingredient in Ingredients())
        {
            var tags = ingredient.ingredientEffects;
            for (int i = 0; i < tags.Count; i++)
                for (int j = 0; j < tags.Count; j++)
                    if (tags[i].tag_effect != tags[j].tag_effect) pairs.Add((tags[i].tag_effect, tags[j].tag_effect));
        }
        if (ordered_tags.Count >= 2) pairs.RemoveAll(p => p.main == ordered_tags[0].tag_effect && p.lesser == ordered_tags[1].tag_effect);
        if (pairs.Count == 0) return;

        var pick = pairs[Random.Range(0, pairs.Count)];
        ordered_tags = new List<IngredientTags>
        {
            new IngredientTags { tag_effect = pick.main, tag_value = 2 },
            new IngredientTags { tag_effect = pick.lesser, tag_value = 1 },
        };
        SetOrderVariables();
    }

    private IEnumerable<IngredientData> Ingredients()
    {
        if (availableIngredients != null)
        {
            foreach (var entry in availableIngredients.entries)
                if (entry != null && entry.ingredient != null) yield return entry.ingredient;
            yield break;
        }
        foreach (var ingredient in FindObjectsByType<Ingredient>())
            if (ingredient.data != null) yield return ingredient.data;
    }

    private void SetOrderVariables()
    {
        SetVariable("$orderMain", ordered_tags.Count > 0 ? ordered_tags[0].tag_effect.ToString() : "");
        SetVariable("$orderLesserName", ordered_tags.Count > 1 ? IngredientTags.Name(ordered_tags[1].tag_effect).ToLower() : "");
        SetVariable("$potionsGiven", submissions.Count);
    }

    private static string Describe(List<IngredientTags> tags) =>
        string.Join(" + ", tags.ConvertAll(t => IngredientTags.Name(t.tag_effect)));

    private static string DescribeValues(List<IngredientTags> tags) =>
        tags.Count > 0 ? string.Join(", ", tags.ConvertAll(t => $"{IngredientTags.Name(t.tag_effect)} {t.tag_value}")) : "no tags";

    private void SetVariable(string name, bool value) { if (variableStorage != null) variableStorage.SetValue(name, value); }
    private void SetVariable(string name, float value) { if (variableStorage != null) variableStorage.SetValue(name, value); }
    private void SetVariable(string name, string value) { if (variableStorage != null) variableStorage.SetValue(name, value); }
}
