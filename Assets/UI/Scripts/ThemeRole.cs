using UnityEngine;

namespace Nocturne.UI
{
    public class ThemeRole : MonoBehaviour
    {
        public enum Role { Auto, Ignore, Panel, Card, Button, Tab, Body, Title, Muted, Accent, Gold }

        public Role role = Role.Auto;
    }
}
