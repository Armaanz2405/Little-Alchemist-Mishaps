using Nocturne.PotionMaking.Energy;
using Nocturne.UI;
using UnityEngine;

// cutting board prompt, place an ingredient then chop it
[RequireComponent(typeof(AlchemyStation))]
public class AlchemyPrompt : StationPrompt
{
    [SerializeField] private string placeText = "Place in beaker";

    private AlchemyStation alchemy;
    private Collider2D alchemyArea;

    protected override void Awake()
    {
        base.Awake();
        alchemy = GetComponent<AlchemyStation>();
        alchemyArea = GetComponent<Collider2D>();
    }

    public override bool Fill(PromptContext context, PromptContent content)
    {
        content.header = title;
        bool overBoard = alchemyArea == null || Mathf.Abs(context.position.x - alchemyArea.bounds.center.x) <= alchemyArea.bounds.extents.x;
        if (context.holding && !alchemy.HasIngredient && overBoard)
            content.lines.Add(new PromptLine(PickKey(context, actionKey, padKey), placeText));
        else if (!context.holding && station != null && (station.CanPlay || station.BlockedByEnergy))
            content.lines.Add(new PromptLine(station.Key.ToString(), actionText, Cost(EnergyAction.Refine)));
        return content.lines.Count > 0;
    }
}
