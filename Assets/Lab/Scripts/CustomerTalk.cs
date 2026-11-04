using System.Collections.Generic;
using Nocturne.PotionMaking.Energy;
using Nocturne.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Yarn.Unity;

// walk up to customer and press E to talk
public class CustomerTalk : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string startNode = "KnightDialogueBranch1";
    [SerializeField] private Transform player;
    [SerializeField] private float range = 3f;
    [SerializeField] private Collider2D talkZone;
    [SerializeField] private InputAction talkAction = DefaultTalkAction();

    public UnityEvent onTalk = new UnityEvent();

    private static readonly List<CustomerTalk> all = new List<CustomerTalk>();
    private static readonly HashSet<string> askedTopics = new HashSet<string>();
    private PlayerInput playerInput;

    public static bool AnyCanTalk()
    {
        foreach (var customer in all)
        {
            if (customer.CanTalk())
            {
                return true;
            }
        }
        return false;
    }

    private static InputAction DefaultTalkAction()
    {
        InputAction action = new InputAction("Talk", InputActionType.Button, "<Keyboard>/e");
        action.AddBinding("<Gamepad>/buttonEast");
        return action;
    }

    private void Awake()
    {
        askedTopics.Clear();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null)
            {
                player = found.transform;
            }
        }
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
        }
        if (dialogueRunner != null && dialogueRunner.onDialogueComplete != null)
        {
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        }
    }

    private void OnEnable()
    {
        all.Add(this);
        talkAction.performed += OnTalkPressed;
        talkAction.Enable();
    }

    private void OnDisable()
    {
        all.Remove(this);
        talkAction.performed -= OnTalkPressed;
        talkAction.Disable();
    }

    public bool CanTalk()
    {
        if (dialogueRunner == null || player == null || dialogueRunner.IsDialogueRunning)
        {
            return false;
        }
        if (talkZone != null)
        {
            return talkZone.OverlapPoint(player.position);
        }
        return Mathf.Abs(player.position.x - transform.position.x) <= range;
    }

    private void OnTalkPressed(InputAction.CallbackContext context)
    {
        if (!CanTalk())
        {
            return;
        }
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }

        dialogueRunner.StartDialogue(startNode);
    }

    [YarnFunction("can_talk")]
    public static bool CanTalkAbout(string topic) => askedTopics.Contains(topic) || PromptSource.CanAfford(EnergyAction.Talk);

    [YarnFunction("talk_cost")]
    public static string TalkCost(string topic)
    {
        string cost = askedTopics.Contains(topic) ? null : PromptSource.Cost(EnergyAction.Talk);
        return cost != null ? $"  <color={UITheme.Hex(UITheme.Current.goldText)}>{cost}</color>" : "";
    }

    [YarnCommand("spend_talk_amount")]
    public void SpendTalkAmount(string topic)
    {
        if (!askedTopics.Add(topic)) return;
        onTalk.Invoke();
    }

    private void OnDialogueComplete()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
    }
}
