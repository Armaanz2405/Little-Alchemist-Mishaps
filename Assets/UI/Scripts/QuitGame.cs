using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitApplication()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Closes the built application
            Application.Quit();
        #endif
    }
}