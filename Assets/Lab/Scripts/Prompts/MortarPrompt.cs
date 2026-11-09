using Nocturne.PotionMaking.Energy;
using Nocturne.UI;
using UnityEngine;

// mortar prompt with how many ingredients are in it, then the grind once it's full
[RequireComponent(typeof(MortarNPestle))]
public class MortarPrompt : StationPrompt
{
    [SerializeField] private string grindText = "Grind";
    [SerializeField] private string checkText = "Check";

    private MortarNPestle mortar;

    protected override void Awake()
    {
        base.Awake();
        mortar = GetComponent<MortarNPestle>();
    }

    public override bool Fill(PromptContext context, PromptContent content)
    {
        content.header = title;
        content.counter = Counter();
        if (context.holding && !mortar.IsFull)
            content.lines.Add(new PromptLine(PickKey(context, actionKey, padKey), actionText, ActionCost()));
        else if (!context.holding && station != null && (station.CanPlay || station.BlockedByEnergy))
            content.lines.Add(new PromptLine(station.Key.ToString(), mortar.IsFull ? grindText : checkText, mortar.IsFull ? Cost(EnergyAction.Refine) : null));
        return content.lines.Count > 0 || showWhenIdle;
    }

    protected override string Counter() => $"{mortar.Contents.Count}/{MortarNPestle.MaxIngredientsPerCrush}";

    // without a grind minigame the last ingredient crushes them all, so that's where the cost shows
    protected override string ActionCost() =>
        !mortar.HasGrindMinigame && mortar.Contents.Count == MortarNPestle.MaxIngredientsPerCrush - 1 ? Cost(EnergyAction.Refine) : null;
}
