using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeBookPage : MonoBehaviour
{
    public void LoadTargetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
