using UnityEngine;

public class ControlUI : MonoBehaviour
{
    [SerializeField] private GameObject keyboardScreen;
    [SerializeField] private GameObject controllerScreen;
    [SerializeField] private GameObject chooseScreen;

    void Awake()
    {
        Time.timeScale = 0f;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openKey()
    {
        keyboardScreen.SetActive(true);
    }
    public void openCon()
    {
        controllerScreen.SetActive(true);
    }
    public void closeKey()
    {
        keyboardScreen.SetActive(false);
        Time.timeScale = 1f;
    }
    public void closeCon()
    {
        controllerScreen.SetActive(false);
        Time.timeScale = 1f;
    }
    public void closeChoose()
    {
        chooseScreen.SetActive(false);
    }
    
    
}
