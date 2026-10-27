using UnityEngine;
using UnityEngine.UI;

// Put this on every panel (MainPanel, OptionsPanel, any sub-panel).
public class MenuPanel : MonoBehaviour
{
    [Tooltip("What gets selected when this panel opens for the first time")]
    public Selectable firstSelected;
}