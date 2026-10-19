using System;
using System.Collections.Generic;
using UnityEngine;

// The list of "ingredient 1 + ingredient 2 = potion" recipes. Edit it in the Inspector.
// Create via Assets > Create > Potion Making > Recipe Book.
[CreateAssetMenu(fileName = "RecipeBook", menuName = "Potion Making/Recipe Book")]
public class PotionRecipeBook : ScriptableObject
{
    [Serializable]
    public class Recipe
    {
        public IngredientData first;
        public IngredientData second;
        public PotionData result;
    }

    public List<Recipe> recipes = new List<Recipe>();

    [Tooltip("What you get when the two ingredients don't match any recipe.")]
    public PotionData failedPotion;

    // Order doesn't matter: A + B and B + A make the same potion.
    public PotionData Combine(IngredientData a, IngredientData b)
    {
        foreach (var recipe in recipes)
        {
            if (recipe == null || recipe.result == null) continue;
            bool match = (recipe.first == a && recipe.second == b) || (recipe.first == b && recipe.second == a);
            if (match) return recipe.result;
        }
        return failedPotion;
    }
}
