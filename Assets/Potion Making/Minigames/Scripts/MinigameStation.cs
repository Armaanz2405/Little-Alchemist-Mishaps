using Nocturne.PotionMaking.Energy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    // Stand next to it and press E to play its minigame. Player input is paused while it runs.
    // onPlayedWithIngredient passes the ingredient you were holding (if any) so the cutting board / cauldron can use the score.
    public class MinigameStation : MonoBehaviour
    {
        // what happens when the player can't afford it
        public enum EnergyRule { Free, Block, ZeroScore }

        [SerializeField] private Minigame minigame;
        [SerializeField] private Transform player;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float range = 1.8f;
        [SerializeField] private Key key = Key.E;
        [SerializeField] private string prompt = "E: play";
        [SerializeField] private bool showLabel = true;
        // turn on for stations that something else unlocks (like the cutting board) by setting isReadyToPlay
        [SerializeField] private bool waitUntilReady = false;
        [SerializeField] private EnergyRule energyRule = EnergyRule.Free;
        [SerializeField] private EnergyAction energyAction = EnergyAction.Refine;

        public bool isReadyToPlay = false;

        public float Range => range;
        public Key Key => key;
        public bool CanPlay => Ready && (energyRule != EnergyRule.Block || HasEnergy);
        public bool BlockedByEnergy => Ready && energyRule == EnergyRule.Block && !HasEnergy;
        public bool HasEnergy => energyRule == EnergyRule.Free || energy == null || energy.CanAfford(energyAction);
        private bool Ready => minigame != null && !minigame.Running && (!waitUntilReady || isReadyToPlay);

        public UnityEvent<float> onPlayed = new UnityEvent<float>();
        public UnityEvent<IngredientData, float> onPlayedWithIngredient = new UnityEvent<IngredientData, float>();

        private TextMesh label;
        private PlayerInput playerInput;
        private EnergyMeter energy;

        private void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (player != null) playerInput = player.GetComponent<PlayerInput>();
            energy = FindAnyObjectByType<EnergyMeter>();
            label = MakeLabel();
        }

        private void Update()
        {
            if (minigame == null || player == null) return;
            bool near = CanPlay && Vector2.Distance(player.position, transform.position) <= range;
            label.gameObject.SetActive(near && showLabel);
            var kb = Keyboard.current;
            if (near && kb != null && kb[key].wasPressedThisFrame) Play();
        }

        public void Play()
        {
            isReadyToPlay = false;
            label.gameObject.SetActive(false);

            var held = holdPoint != null ? holdPoint.GetComponentInChildren<Ingredient>() : null;
            var data = held != null ? held.data : null;
            if (data != null && minigame is KnifeSliceMinigame knife) knife.SetIngredientColor(data.color);

            bool outOfEnergy = energyRule == EnergyRule.ZeroScore && !HasEnergy;
            if (playerInput != null) playerInput.DeactivateInput();
            minigame.Begin(outOfEnergy: outOfEnergy, onDone: score =>
            {
                if (playerInput != null) playerInput.ActivateInput();
                onPlayed.Invoke(score);
                onPlayedWithIngredient.Invoke(data, score);
                Debug.Log($"[Minigame] {minigame.Title}: {Mathf.RoundToInt(score * 100)}%" + (data != null ? $" ({data.displayName})" : ""));
            });
        }

        private TextMesh MakeLabel()
        {
            var go = new GameObject("Prompt", typeof(TextMesh));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var tm = go.GetComponent<TextMesh>();
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            go.GetComponent<MeshRenderer>().material = tm.font.material;
            go.GetComponent<MeshRenderer>().sortingOrder = 100;
            tm.text = prompt;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.characterSize = 0.12f;
            tm.fontSize = 48;
            tm.color = Color.white;
            go.SetActive(false);
            return tm;
        }
    }
}
