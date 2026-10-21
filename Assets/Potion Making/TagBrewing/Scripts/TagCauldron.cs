using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.TagBrewing
{
    [RequireComponent(typeof(Collider2D))]
    public class TagCauldron : MonoBehaviour
    {
        [SerializeField] private TagRules rules;
        [SerializeField] private Potion potionPrefab;
        [SerializeField] private Transform potionSpawnPoint;
        [SerializeField] private SpriteRenderer liquid;
        [SerializeField] private Color emptyLiquidColor = new Color(0.24f, 0.2f, 0.27f);
        [Header("Bottling")]
        [SerializeField] private Transform player;
        [SerializeField] private float bottleRange = 2.2f;
        [SerializeField] private Key bottleKey = Key.E;
        [Header("Label above the pot")]
        [SerializeField] private TextMeshPro label;
        [SerializeField] private Vector3 labelOffset = new Vector3(0f, 2.8f, 0f);

        public UnityEvent<IngredientData> onIngredientAdded = new UnityEvent<IngredientData>();
        public UnityEvent<PotionData> onPotionBottled = new UnityEvent<PotionData>();
        public UnityEvent<int> onBubbledOver = new UnityEvent<int>();

        public System.Func<float> roll = () => Random.value;

        private readonly List<IngredientData> contents = new List<IngredientData>();
        private string flashMessage;
        private Coroutine flashRoutine;

        public IReadOnlyList<IngredientData> Contents => contents;
        public Dictionary<IngredientTags.Effects, int> CurrentTags => TagMath.Combine(contents, rules);

        private void Reset() => GetComponent<Collider2D>().isTrigger = true;

        private void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
            if (label == null) label = MakeLabel();
            UpdateVisuals();
        }

        private void Update()
        {
            bool near = player != null && Vector2.Distance(player.position, transform.position) <= bottleRange;
            var kb = Keyboard.current;
            if (near && kb != null && kb[bottleKey].wasPressedThisFrame) Bottle();
            if (label != null) label.text = LabelText(near);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var ingredient = other.GetComponentInParent<Ingredient>();
            if (ingredient == null || ingredient.data == null || !ingredient.TryUse()) return;
            AddIngredient(ingredient.data);
            Destroy(ingredient.gameObject);
        }

        public bool AddIngredient(IngredientData ingredient)
        {
            contents.Add(ingredient);
            onIngredientAdded.Invoke(ingredient);

            if (roll() < TagMath.OverflowChance(contents.Count, rules))
            {
                int lost = contents.Count;
                contents.Clear();
                Debug.Log($"[TagCauldron] Bubbled over! Lost {lost} ingredients.");
                onBubbledOver.Invoke(lost);
                Flash($"It bubbled over! Lost {lost} ingredients");
                UpdateVisuals();
                return false;
            }

            Debug.Log($"[TagCauldron] Added {ingredient.displayName} -> {TagMath.Describe(CurrentTags)}");
            UpdateVisuals();
            return true;
        }

        public PotionData Bottle()
        {
            if (contents.Count == 0) return null;
            var tags = CurrentTags;
            contents.Clear();

            var result = ScriptableObject.CreateInstance<PotionData>();
            var top = TagMath.Dominant(tags);
            result.displayName = top.HasValue ? $"{TagMath.Nice(top.Value)} Potion" : "Murky Sludge";
            result.color = top.HasValue && rules != null ? rules.ColorFor(top.Value) : new Color(0.36f, 0.32f, 0.27f);
            result.name = result.displayName + (tags.Count > 0 ? $" ({TagMath.Describe(tags)})" : "");

            if (potionPrefab != null)
            {
                Vector3 at = potionSpawnPoint != null ? potionSpawnPoint.position : transform.position + Vector3.up;
                var potion = Instantiate(potionPrefab, at, Quaternion.identity);
                potion.data = result;
                potion.name = result.name;
                potion.ApplyLook();
            }
            Debug.Log($"[TagCauldron] Bottled {result.name}");
            onPotionBottled.Invoke(result);
            Flash($"Bottled: {result.name}");
            UpdateVisuals();
            return result;
        }

        private void UpdateVisuals()
        {
            if (liquid == null) return;
            if (contents.Count == 0) { liquid.color = emptyLiquidColor; return; }
            Color mix = Color.black;
            foreach (var i in contents) mix += i.color;
            liquid.color = mix / contents.Count;
        }

        private string LabelText(bool near)
        {
            if (!string.IsNullOrEmpty(flashMessage)) return flashMessage;
            if (contents.Count == 0) return near ? "<size=70%>drop ingredients in</size>" : "";
            string tags = TagMath.Describe(CurrentTags);
            if (string.IsNullOrEmpty(tags)) tags = "<i>everything cancelled out</i>";
            string risk = TagMath.OverflowChance(contents.Count + 1, rules) > 0f ? "\n<size=60%><color=#ffb15c>bubbling... adding more is risky</color></size>" : "";
            string hint = near ? $"\n<size=60%>{bottleKey}: bottle</size>" : "";
            return $"{tags}{risk}{hint}";
        }

        private void Flash(string message)
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(message));
        }

        private IEnumerator FlashRoutine(string message)
        {
            flashMessage = message;
            yield return new WaitForSeconds(2f);
            flashMessage = null;
        }

        private TextMeshPro MakeLabel()
        {
            var go = new GameObject("TagLabel", typeof(TextMeshPro));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = labelOffset;
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.fontSize = 4;
            tmp.alignment = TextAlignmentOptions.Bottom;
            tmp.rectTransform.sizeDelta = new Vector2(10, 3);
            tmp.color = Color.white;
            tmp.sortingOrder = 100;
            return tmp;
        }
    }
}
