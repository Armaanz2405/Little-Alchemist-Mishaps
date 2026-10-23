using Nocturne.PotionMaking.Energy;
using Nocturne.PotionMaking.Minigames;
using Nocturne.UI;
using UnityEngine;

// cauldron prompt with how full it is, adding ingredients and stirring
public class CauldronPrompt : StationPrompt
{
    [SerializeField] private Cauldron cauldron;
    [SerializeField] private PuzzleLogic_Cauldron puzzleCauldron;
    [SerializeField] private MinigameStation stirStation;
    [SerializeField] private string stirText = "Stir";

    protected override void Awake()
    {
        base.Awake();
        if (cauldron == null && puzzleCauldron == null)
        {
            cauldron = Nearest<Cauldron>();
            puzzleCauldron = Nearest<PuzzleLogic_Cauldron>();
        }
        Component found = puzzleCauldron != null ? puzzleCauldron : cauldron;
        if (found != null) area = found.GetComponent<Collider2D>();
    }

    protected override string Counter()
    {
        if (puzzleCauldron != null) return $"{puzzleCauldron.Contents.Count}/{PuzzleLogic_Cauldron.IngredientsPerPotion}";
        if (cauldron != null) return $"{cauldron.Contents.Count}/{Cauldron.IngredientsPerPotion}";
        return null;
    }

    protected override void AddLines(PromptContext context, PromptContent content)
    {
        bool inStirRange = stirStation != null && Vector2.Distance(stirStation.transform.position, context.position) <= stirStation.Range;
        if (!context.holding && inStirRange && stirStation.CanPlay)
            content.lines.Add(new PromptLine(stirStation.Key.ToString(), stirText, CanAfford(EnergyAction.Bottle) ? Cost(EnergyAction.Bottle) : null));
    }

    private T Nearest<T>() where T : Component
    {
        T best = null;
        float bestDist = float.MaxValue;
        foreach (var c in FindObjectsByType<T>())
        {
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c; }
        }
        return best;
    }
}
