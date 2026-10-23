using UnityEngine;

namespace Nocturne.PotionMaking.Energy
{
    public enum EnergyAction { Inspect, Refine, Bottle, Talk }

    [CreateAssetMenu(fileName = "EnergyCosts", menuName = "Potion Making/Energy Costs")]
    public class EnergyCosts : ScriptableObject
    {
        [Min(0)] public int inspect = 1;
        [Min(0)] public int refine = 1;
        [Min(0)] public int bottle = 1;
        [Min(0)] public int talk = 1;

        public int CostOf(EnergyAction action)
        {
            switch (action)
            {
                case EnergyAction.Inspect: return inspect;
                case EnergyAction.Refine: return refine;
                case EnergyAction.Bottle: return bottle;
                case EnergyAction.Talk: return talk;
                default: return 1;
            }
        }
    }
}
