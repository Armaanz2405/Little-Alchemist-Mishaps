using Nocturne.PotionMaking.Minigames;
using Nocturne.UI;
using UnityEngine;

// cauldron prompt with how full it is, adding ingredients and stirring
public class CauldronPrompt : StationPrompt
{
    [SerializeField] private Cauldron cauldron;
    [SerializeField] private MinigameStation stirStation;
    [SerializeField] private string stirText = "Stir";

    protected override void Awake()
    {
        base.Awake();
        if (cauldron == null) cauldron = Nearest();
        if (cauldron != null) area = cauldron.GetComponent<Collider2D>();
    }

    protected override string Counter() => cauldron != null ? $"{cauldron.Contents.Count}/{Cauldron.IngredientsPerPotion}" : null;

    protected override void AddLines(PromptContext context, PromptContent content)
    {
        if (!context.holding && stirStation != null && stirStation.CanPlay)
            content.lines.Add(new PromptLine(stirStation.Key.ToString(), stirText));
    }

    private Cauldron Nearest()
    {
        Cauldron best = null;
        float bestDist = float.MaxValue;
        foreach (var c in FindObjectsByType<Cauldron>())
        {
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c; }
        }
        return best;
    }
}
