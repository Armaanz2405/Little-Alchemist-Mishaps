using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nocturne.PotionMaking.Discovery
{
    public class DiscoveryLog : MonoBehaviour
    {
        [SerializeField] private List<IngredientData> knownAtStart = new List<IngredientData>();

        public UnityEvent<IngredientData> onDiscovered = new UnityEvent<IngredientData>();

        private readonly HashSet<IngredientData> discovered = new HashSet<IngredientData>();

        public int Count => discovered.Count;

        private void Awake()
        {
            foreach (var ingredient in knownAtStart)
                if (ingredient != null) discovered.Add(ingredient);
        }

        public bool IsDiscovered(IngredientData ingredient) => ingredient != null && discovered.Contains(ingredient);

        public bool Discover(IngredientData ingredient)
        {
            if (ingredient == null || !discovered.Add(ingredient)) return false;
            onDiscovered.Invoke(ingredient);
            return true;
        }
    }
}
