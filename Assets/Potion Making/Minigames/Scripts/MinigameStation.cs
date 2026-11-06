using System.Collections.Generic;
using Nocturne.PotionMaking.Energy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    // Stand next to it and press E (B on a pad) to play its minigame. Player input is paused while it runs.
    // onPlayedWithIngredient passes the ingredient you were holding (if any) so the cutting board / cauldron can use the score.
    public class MinigameStation : MonoBehaviour
    {
        // what happens when the player can't afford it
        public enum EnergyRule { Free, Block, ZeroScore }
        // which A/B setting decides between minigame and alternate
        public enum VariantSlot { None, Cauldron, Chop }

        [SerializeField] private Minigame minigame;
        [Tooltip("Optional second version of the game. The slot's setting in MinigameVariants picks which one runs.")]
        [SerializeField] private Minigame alternate;
        [SerializeField] private VariantSlot slot = VariantSlot.None;
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

        // the version that's live right now
        public Minigame Game
        {
            get
            {
                if (alternate == null) return minigame;
                switch (slot)
                {
                    case VariantSlot.Cauldron: return MinigameVariants.Cauldron == CauldronVariant.Brew ? alternate : minigame;
                    case VariantSlot.Chop: return MinigameVariants.Chop == ChopVariant.Rhythm ? alternate : minigame;
                    default: return minigame;
                }
            }
        }

        // both versions, for stations that have to hand something to whichever one ends up running
        public IEnumerable<Minigame> AllGames
        {
            get
            {
                if (minigame != null) yield return minigame;
                if (alternate != null && alternate != minigame) yield return alternate;
            }
        }

        public float Range => range;
        public Key Key => key;
        public bool CanPlay => Ready && (energyRule != EnergyRule.Block || HasEnergy);
        public bool BlockedByEnergy => Ready && energyRule == EnergyRule.Block && !HasEnergy;
        public bool HasEnergy => energyRule == EnergyRule.Free || energy == null || energy.CanAfford(energyAction);
        private bool Ready => Game != null && !AnyRunning && (!waitUntilReady || isReadyToPlay);
        private bool AnyRunning
        {
            get
            {
                foreach (var g in AllGames) if (g.Running) return true;
                return false;
            }
        }

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
            if (Game == null || player == null) return;
            bool near = CanPlay && Vector2.Distance(player.position, transform.position) <= range;
            label.gameObject.SetActive(near && showLabel);
            if (near && MinigameInput.StationPressed(key)) Play();
        }

        public void Play()
        {
            var game = Game;
            if (game == null) return;
            isReadyToPlay = false;
            label.gameObject.SetActive(false);

            var held = holdPoint != null ? holdPoint.GetComponentInChildren<Ingredient>() : null;
            var data = held != null ? held.data : null;
            // the cutting board sets its game's ingredient itself, so only override it when something is actually held
            if (data != null && game is ITagChoiceMinigame choice) choice.SetIngredient(data);

            bool outOfEnergy = energyRule == EnergyRule.ZeroScore && !HasEnergy;
            if (playerInput != null) playerInput.DeactivateInput();
            game.Begin(outOfEnergy: outOfEnergy, onDone: score =>
            {
                if (playerInput != null) playerInput.ActivateInput();
                onPlayed.Invoke(score);
                onPlayedWithIngredient.Invoke(data, score);
                Debug.Log($"[Minigame] {game.Title}: {Mathf.RoundToInt(score * 100)}%" + (data != null ? $" ({data.displayName})" : ""));
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
