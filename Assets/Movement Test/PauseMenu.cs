using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Put this on the PauseMenu object (the one with the CanvasGroup).
public class PauseMenu : MonoBehaviour
{
    private int lastToggleFrame = -1;
    
    // One entry in the navigation history.
    private class Page
    {
        public MenuPanel panel;
        public GameObject lastSelected; // so Back() restores the highlight
    }

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private MenuPanel mainPanel;   // the first page shown when pausing

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private string mainMenuScene = "MainMenu";

    public static bool IsPaused { get; private set; }

    // Last node = the panel currently on screen.
    private readonly LinkedList<Page> history = new LinkedList<Page>();
    private Coroutine fadeRoutine;

    private void Awake()
    {
        SetVisible(false);
        IsPaused = false;
    }
    // ---------- pause / resume ----------

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        history.Clear();
        OpenPanel(mainPanel);
        Fade(1f);
        InputManager.instanceInput.EnableUI();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        foreach (Page p in history) p.panel.gameObject.SetActive(false);
        history.Clear();
        Fade(0f);
        InputManager.instanceInput.EnableGameplay();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        IsPaused = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    // ---------- navigation (linked list as a history) ----------

    // Hook this to button OnClick events: drag the target panel into the slot.
    public void OpenPanel(MenuPanel panel)
    {
        // Hide the current panel and remember what was selected on it.
        if (history.Count > 0)
        {
            Page current = history.Last.Value;
            current.lastSelected = EventSystem.current.currentSelectedGameObject;
            current.panel.gameObject.SetActive(false);
        }

        history.AddLast(new Page { panel = panel });
        panel.gameObject.SetActive(true);
        Select(panel.firstSelected != null ? panel.firstSelected.gameObject : null);
    }

    // Hook this to Back buttons too.
    public void Back()
    {
        if (history.Count <= 1) return;

        history.Last.Value.panel.gameObject.SetActive(false);
        history.RemoveLast();

        Page previous = history.Last.Value;
        previous.panel.gameObject.SetActive(true);

        GameObject toSelect = previous.lastSelected;
        if (toSelect == null && previous.panel.firstSelected != null)
            toSelect = previous.panel.firstSelected.gameObject;
        Select(toSelect);
    }

    // ---------- helpers ----------

    private void Select(GameObject go)
    {
        EventSystem.current.SetSelectedGameObject(null); // clear first, or re-selecting does nothing
        if (go != null) EventSystem.current.SetSelectedGameObject(go);
    }

    private void Fade(float target)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        if (target > 0f) { canvasGroup.blocksRaycasts = true; canvasGroup.interactable = true; }

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        SetVisible(target > 0f);
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }
    public void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Time.frameCount == lastToggleFrame) return; // ignore the press that just opened/closed the menu
        Debug.Log($"OnCancel phase={context.phase} paused={IsPaused} history={history.Count}");
        if (!IsPaused) Pause();
        else if (history.Count > 1) Back();
        else Resume();
    }
}
