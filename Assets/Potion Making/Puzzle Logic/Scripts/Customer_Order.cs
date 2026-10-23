using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Customer_Order : MonoBehaviour
{
    public List<IngredientTags> ordered_tags = new List<IngredientTags>();
    //[HideInInspector] public IngredientTags main_effect = ordered_tags[0];
    private List<IngredientTags> submitted_potion = new List<IngredientTags>();
    [SerializeField] private VariableStorageBehaviour variableStorage;

    void Start()
    {
        
    }

    public void SetSubmittedPotion(List<IngredientTags> tags)
    {
        submitted_potion = new List<IngredientTags>(tags);
    }

    [YarnCommand("grade_potion")]
    public void GradePotion()
    {
        for (int i = 0; i < ordered_tags.Count; i++)
        {
            if (!submitted_potion.Contains(ordered_tags[i]))
            {
                variableStorage.SetValue("$isPotionPassable", false);
                return;
            }
        }
        variableStorage.SetValue("$isHoldingPotion", true);
        return;
    }
}
