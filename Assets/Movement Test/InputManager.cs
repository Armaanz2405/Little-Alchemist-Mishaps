using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{  
    public static InputManager instanceInput { get; private set; }
    public PlayerInput playerInput;
    private void Awake()
    {
        if (instanceInput != null && instanceInput != this)
        {
            Destroy(gameObject);
            return;
        }

        instanceInput = this;
    }
    public void EnableUI()
    {
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void EnableGameplay()
    {
        playerInput.SwitchCurrentActionMap("Movement");
    }
}

