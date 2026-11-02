using UnityEngine;
using Yarn.Unity;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ClickToTalk : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string startNode = "KnightDialogueBranch1";

    void Update()
    {
        bool clicked = false;
        Vector2 screenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            clicked = true;
            screenPos = Mouse.current.position.ReadValue();
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            clicked = true;
            screenPos = Input.mousePosition;
        }
#endif

        if (!clicked) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);

        bool clickedMe = false;
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
            {
                clickedMe = true;
                break;
            }
        }

        if (!clickedMe) return;
        if (dialogueRunner.IsDialogueRunning) return;

        dialogueRunner.StartDialogue(startNode);
    }
}