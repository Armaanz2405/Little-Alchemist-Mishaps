using Nocturne.PotionMaking.Energy;
using Nocturne.UI;
using UnityEngine;

// cutting board prompt, place an ingredient then chop it
[RequireComponent(typeof(CuttingBoard))]
public class CuttingBoardPrompt : StationPrompt
{
    [SerializeField] private string placeText = "Place on board";

    private CuttingBoard board;
    private Collider2D boardArea;

    protected override void Awake()
    {
        base.Awake();
        board = GetComponent<CuttingBoard>();
        boardArea = GetComponent<Collider2D>();
    }

    public override bool Fill(PromptContext context, PromptContent content)
    {
        content.header = title;
        bool overBoard = boardArea == null || Mathf.Abs(context.position.x - boardArea.bounds.center.x) <= boardArea.bounds.extents.x;
        if (context.holding && !board.HasIngredient && overBoard)
            content.lines.Add(new PromptLine(PickKey(context, actionKey, padKey), placeText));
        else if (!context.holding && station != null && (station.CanPlay || station.BlockedByEnergy))
            content.lines.Add(new PromptLine(station.Key.ToString(), actionText, Cost(EnergyAction.Refine)));
        return content.lines.Count > 0;
    }
}
