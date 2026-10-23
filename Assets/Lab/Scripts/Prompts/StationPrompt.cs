using Nocturne.PotionMaking.Energy;
using Nocturne.PotionMaking.Minigames;
using Nocturne.UI;
using UnityEngine;

// prompt for a work station
public class StationPrompt : PromptSource
{
    [SerializeField] protected string title = "Station";
    [SerializeField] protected string actionText = "Drop in";
    [SerializeField] protected string actionKey = "E";
    [SerializeField] protected string padKey = "B";
    [SerializeField] protected bool needsIngredient = true;
    // still show the header and counter when the player can't do anything yet
    [SerializeField] protected bool showWhenIdle = false;
    // when set, the action only shows while this minigame can be played, using its key and range
    [SerializeField] protected MinigameStation station;

    public override float Distance(Vector2 player)
    {
        if (station == null) return base.Distance(player);
        float d = Vector2.Distance(station.transform.position, player);
        return d <= station.Range ? d : float.PositiveInfinity;
    }

    public override bool Fill(PromptContext context, PromptContent content)
    {
        bool canAct = (!needsIngredient || context.holding) && (station == null || station.CanPlay || station.BlockedByEnergy);
        content.header = title;
        content.counter = Counter();
        if (canAct)
        {
            string key = station != null ? station.Key.ToString() : PickKey(context, actionKey, padKey);
            content.lines.Add(new PromptLine(key, actionText, ActionCost()));
        }
        AddLines(context, content);
        return content.lines.Count > 0 || showWhenIdle;
    }

    protected virtual string Counter() => null;

    protected virtual string ActionCost() => station != null ? Cost(EnergyAction.Refine) : null;

    protected virtual void AddLines(PromptContext context, PromptContent content) { }
}
