using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    // One entry in the navigation history.
    private class Page
    {
        public MenuPanel panel;
        public GameObject lastSelected; // so Back() restores the highlight
    }

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private MenuPanel rootPanel;   // hub page with the Audio / Video / Keyboard / Controller buttons

    [Header("Events")]
    public UnityEvent onClosed; // e.g. re-select the Settings button on the title screen

    public bool IsOpen { get; private set; }

    // Last node = the panel currently on screen.
    private readonly LinkedList<Page> history = new LinkedList<Page>();
    private int lastToggleFrame = -1;

    private void Awake()
    {
        SetVisible(false);
        IsOpen = false;
    }

    // ---------- open / close ----------

    // Hook this to the title screen's "Settings" button.
    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        lastToggleFrame = Time.frameCount;

        history.Clear();
        SetVisible(true);
        OpenPanel(rootPanel);
    }

    // Hook this to the hub page's Back/Close button, or call from OnCancel.
    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        lastToggleFrame = Time.frameCount;

        foreach (Page p in history) p.panel.gameObject.SetActive(false);
        history.Clear();
        SetVisible(false);
        onClosed?.Invoke();
    }

    // ---------- navigation (linked list as a history) ----------

    // Hook to the Audio / Video / Keyboard / Controller buttons: drag the target panel into the slot.
    public void OpenPanel(MenuPanel panel)
    {
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

    // Hook this to the Back buttons on the sub-panels.
    public void Back()
    {
        if (history.Count <= 1) { Close(); return; }

        history.Last.Value.panel.gameObject.SetActive(false);
        history.RemoveLast();

        Page previous = history.Last.Value;
        previous.panel.gameObject.SetActive(true);

        GameObject toSelect = previous.lastSelected;
        if (toSelect == null && previous.panel.firstSelected != null)
            toSelect = previous.panel.firstSelected.gameObject;
        Select(toSelect);
    }

    // Hook this to your UI "Cancel" action (Escape / B button).
    public void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.performed || !IsOpen || Time.frameCount == lastToggleFrame) return;
        Back(); // goes back a page, or closes if you're on the hub
    }

    // ---------- helpers ----------

    private void Select(GameObject go)
    {
        EventSystem.current.SetSelectedGameObject(null); // clear first, or re-selecting does nothing
        if (go != null) EventSystem.current.SetSelectedGameObject(go);
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }
}