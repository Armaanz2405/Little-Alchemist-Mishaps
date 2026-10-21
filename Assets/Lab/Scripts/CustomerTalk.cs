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
    [SerializeField] private InputAction talkAction = DefaultTalkAction();

    public UnityEvent onTalk = new UnityEvent();

    private PlayerInput playerInput;

    private static InputAction DefaultTalkAction()
    {
        InputAction action = new InputAction("Talk", InputActionType.Button, "<Keyboard>/e");
        action.AddBinding("<Gamepad>/buttonEast");
        return action;
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
        talkAction.performed += OnTalkPressed;
        talkAction.Enable();
    }

    private void OnDisable()
    {
        talkAction.performed -= OnTalkPressed;
        talkAction.Disable();
    }

    private bool CanTalk()
    {
        return dialogueRunner != null && player != null && !dialogueRunner.IsDialogueRunning
            && Mathf.Abs(player.position.x - transform.position.x) <= range;
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
        onTalk.Invoke();
        dialogueRunner.StartDialogue(startNode);
    }

    private void OnDialogueComplete()
    {
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
    }
}
