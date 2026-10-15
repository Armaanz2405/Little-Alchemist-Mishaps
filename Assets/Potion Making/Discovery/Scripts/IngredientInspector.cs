using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Discovery
{
    public class IngredientInspector : MonoBehaviour
    {
        [SerializeField] private DiscoveryLog log;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float reach = 0.9f;
        [SerializeField] private Key inspectKey = Key.R;

        public UnityEvent<IngredientData> onInspected = new UnityEvent<IngredientData>();

        private void Awake()
        {
            if (log == null) log = FindFirstObjectByType<DiscoveryLog>();
            if (holdPoint == null) holdPoint = transform;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb[inspectKey].wasPressedThisFrame) InspectNearest();
        }

        public IngredientData InspectNearest()
        {
            var target = FindTarget();
            if (target == null || target.data == null) return null;
            if (log != null) log.Discover(target.data);
            onInspected.Invoke(target.data);
            return target.data;
        }

        private Ingredient FindTarget()
        {
            var held = holdPoint.GetComponentInChildren<Ingredient>();
            if (held != null) return held;

            Ingredient best = null;
            float bestDist = float.MaxValue;
            foreach (var hit in Physics2D.OverlapCircleAll(holdPoint.position, reach))
            {
                var ingredient = hit.GetComponentInParent<Ingredient>();
                if (ingredient == null) continue;
                float d = (ingredient.transform.position - holdPoint.position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = ingredient; }
            }
            return best;
        }
    }
}
