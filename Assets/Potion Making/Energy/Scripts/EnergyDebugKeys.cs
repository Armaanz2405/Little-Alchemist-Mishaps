using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Energy
{
    public class EnergyDebugKeys : MonoBehaviour
    {
        [SerializeField] private EnergyMeter meter;
        [SerializeField] private CustomerEnergy customers;
        [SerializeField] private string keyNpcToTest = "Librarian";

        private void Awake() { if (meter == null) meter = FindFirstObjectByType<EnergyMeter>(); }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || meter == null) return;
            if (kb.digit1Key.wasPressedThisFrame) meter.Spend(1);
            if (kb.digit2Key.wasPressedThisFrame) meter.Refill(customers != null ? customers.standardEnergy : 5);
            if (kb.digit3Key.wasPressedThisFrame) meter.Refill(customers != null ? customers.EnergyFor(keyNpcToTest) : 8);
        }
    }
}
