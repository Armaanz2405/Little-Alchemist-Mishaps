using System.Collections.Generic;
using UnityEngine;

namespace Nocturne.PotionMaking.Energy
{
    [CreateAssetMenu(fileName = "CustomerEnergy", menuName = "Potion Making/Customer Energy")]
    public class CustomerEnergy : ScriptableObject
    {
        [System.Serializable]
        public class KeyNpc
        {
            public string npcName;
            [Min(0)] public int energy = 6;
        }

        [Min(0)] public int standardEnergy = 5;
        public List<KeyNpc> keyNpcs = new List<KeyNpc>();

        public int EnergyFor(string npcName)
        {
            foreach (var npc in keyNpcs)
                if (npc != null && npc.npcName == npcName) return npc.energy;
            return standardEnergy;
        }
    }
}
