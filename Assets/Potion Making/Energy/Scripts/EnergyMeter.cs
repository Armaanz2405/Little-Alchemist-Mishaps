using UnityEngine;
using UnityEngine.Events;

namespace Nocturne.PotionMaking.Energy
{
    public class EnergyMeter : MonoBehaviour
    {
        [SerializeField] private int maxEnergy = 6;
        [SerializeField] private EnergyCosts costs;

        public UnityEvent<int, int> onChanged = new UnityEvent<int, int>();
        public UnityEvent onOutOfEnergy = new UnityEvent();

        public int Current { get; private set; }
        public int Max => maxEnergy;
        public EnergyCosts Costs => costs;

        private void Awake() => Current = maxEnergy;
        private void Start() => onChanged.Invoke(Current, maxEnergy);

        public bool CanSpend(int cost) => cost >= 0 && cost <= Current;

        public bool TrySpend(int cost)
        {
            if (!CanSpend(cost)) return false;
            if (cost == 0) return true;
            Current -= cost;
            onChanged.Invoke(Current, maxEnergy);
            if (Current == 0) onOutOfEnergy.Invoke();
            return true;
        }

        public bool TrySpend(EnergyAction action) => TrySpend(CostOf(action));

        public int CostOf(EnergyAction action) => costs != null ? costs.CostOf(action) : 1;
        public bool CanAfford(EnergyAction action) => CanSpend(CostOf(action));

        public void Refill(int amount)
        {
            maxEnergy = Mathf.Max(0, amount);
            Current = maxEnergy;
            onChanged.Invoke(Current, maxEnergy);
        }

        public void Spend(int cost) => TrySpend(cost);
        public void SpendInspect() => TrySpend(EnergyAction.Inspect);
        public void SpendRefine() => TrySpend(EnergyAction.Refine);
        public void SpendBottle() => TrySpend(EnergyAction.Bottle);
        public void SpendTalk() => TrySpend(EnergyAction.Talk);
    }
}
