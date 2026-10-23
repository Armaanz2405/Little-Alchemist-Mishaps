using Nocturne.UI;
using UnityEngine;

// prompt when the player can talk to the customer
[RequireComponent(typeof(CustomerTalk))]
public class TalkPrompt : PromptSource
{
    [SerializeField] private string text = "Talk";
    [SerializeField] private string key = "E";
    [SerializeField] private string padKey = "B";
    [SerializeField] private float backLineY = -100f;

    private CustomerTalk talk;

    protected override void Awake()
    {
        base.Awake();
        area = null;
        talk = GetComponent<CustomerTalk>();
    }

    public override float Distance(Vector2 player) =>
        talk != null && talk.CanTalk() ? Mathf.Abs(player.x - transform.position.x) + Mathf.Max(0f, backLineY - player.y) : float.PositiveInfinity;

    public override bool Fill(PromptContext context, PromptContent content)
    {
        content.lines.Add(new PromptLine(PickKey(context, key, padKey), text));
        return true;
    }
}
