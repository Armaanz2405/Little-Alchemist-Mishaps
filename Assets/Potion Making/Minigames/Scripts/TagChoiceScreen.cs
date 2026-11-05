using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // A cutting minigame that first asks which tag to cut away. The cutting board sets the ingredient and reads the choice.
    public interface ITagChoiceMinigame
    {
        void SetIngredient(IngredientData data);
        int ChosenTag { get; }
    }

    // The "which tag do you want to cut away?" picker shared by the chopping games. A/D to pick, confirm to go.
    public class TagChoiceScreen
    {
        public int Chosen { get; private set; }
        public bool Open => root != null;

        private readonly IngredientData ingredient;
        private RectTransform root;
        private TMP_Text pickLabel;
        private readonly List<RectTransform> frames = new List<RectTransform>();

        public TagChoiceScreen(IngredientData ingredient) => this.ingredient = ingredient;

        public void Build(Transform parent)
        {
            root = MinigameUI.Rect(parent, "Choice", new Vector2(0, -20), new Vector2(1000, 560), MinigameUI.Board);
            frames.Clear();
            var tags = ingredient.ingredientEffects;
            const float spacing = 260f;
            for (int i = 0; i < tags.Count; i++)
            {
                float x = (i - (tags.Count - 1) / 2f) * spacing;
                frames.Add(MinigameUI.Rect(root, "Frame", new Vector2(x, 20), new Vector2(232, 192), MinigameUI.Gold));
                var card = MinigameUI.Rect(root, "Tag", new Vector2(x, 20), new Vector2(214, 174), IngredientTags.ColorOf(tags[i].tag_effect));
                MinigameUI.Label(card, $"<b>{IngredientTags.Name(tags[i].tag_effect)}</b>\n<size=22>x{tags[i].tag_value}</size>", Vector2.zero, 26, MinigameUI.Dark, 210);
            }
            pickLabel = MinigameUI.Label(root, "", new Vector2(0, -190), 26, MinigameUI.Ink);
            Refresh();
        }

        // Returns true on the frame the player confirms. Tears the screen down itself.
        public bool Step()
        {
            if (!Open) return false;
            int n = ingredient.ingredientEffects.Count;
            if (MinigameInput.LeftPressed) { Chosen = (Chosen + n - 1) % n; Refresh(); }
            if (MinigameInput.RightPressed) { Chosen = (Chosen + 1) % n; Refresh(); }
            if (!MinigameInput.ConfirmPressed) return false;
            Object.Destroy(root.gameObject);
            root = null;
            return true;
        }

        private void Refresh()
        {
            for (int i = 0; i < frames.Count; i++) frames[i].gameObject.SetActive(i == Chosen);
            var tag = ingredient.ingredientEffects[Chosen];
            pickLabel.text = $"Cut away <b>{IngredientTags.Name(tag.tag_effect)}</b> and keep the rest";
        }
    }
}
