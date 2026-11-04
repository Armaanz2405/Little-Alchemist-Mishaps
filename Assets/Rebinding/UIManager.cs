using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }
    public int lastToggleFrame { get; set; } = -1;
    public EventSystem eventSystem;
    [SerializeField] private GameObject currentUI;
    [SerializeField] private GameObject previousUI;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private PauseMenu pauseMenuScript;
    
    //set instance so it can be called by any script if needed by using UIManager.instance.FUNCTION
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
      
    }
 
    //set a interacble UI to this so that inputs can be done with keyboard or Controller. MUST be called when changing UI screen
    public void setSelected(GameObject selected)
    {
        eventSystem.SetSelectedGameObject(selected);
    }
    //set the current so it can be closed later. MUST be called when changing UI screen. 
    public void setCurrent(GameObject current)
    {
        currentUI=current;
    }

    //set the current so it can be closed later. MUST be called when changing UI screen. 
    public void setPrevious(GameObject previous)
    {
        previousUI=previous;
    }

    public GameObject getCurrent()
    {
        return currentUI;
    }
    //set the previous so that it can be went back to, MUST be called when changing UI screen
    
    
    //goes back to the prevoius ui page or closes ui. 
    public void goBack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Time.frameCount == lastToggleFrame) return; // ignore the press that just opened the menu
        if (currentUI == null) return;

        if (currentUI == pauseScreen)
        {
            Debug.Log("Closing Pause Screen");
         //   pauseMenuScript.closePauseScreen();
            return;
        }
        
        currentUI.SetActive(false);
        previousUI.SetActive(true);
        currentUI = previousUI;
    }
    
}

