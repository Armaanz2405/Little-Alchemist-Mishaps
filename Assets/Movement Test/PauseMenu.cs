using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseScreen;
    
    public void openPauseScreen(InputAction.CallbackContext context)
    {
        if (context.performed
            && Time.frameCount != UIManager.instance.lastToggleFrame
            && UIManager.instance.getCurrent() == null)
        {
            UIManager.instance.lastToggleFrame = Time.frameCount;
            UIManager.instance.setCurrent(pauseScreen);
            pauseScreen.SetActive(true);
            InputManager.instanceInput.EnableUI();
            Time.timeScale = 0;
        }
    }

    public void closePauseScreen()
    {
        if (pauseScreen == UIManager.instance.getCurrent())
        {
            UIManager.instance.lastToggleFrame = Time.frameCount;
            pauseScreen.SetActive(false);
            UIManager.instance.setCurrent(null);
            InputManager.instanceInput.EnableGameplay();
            Time.timeScale = 1;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
