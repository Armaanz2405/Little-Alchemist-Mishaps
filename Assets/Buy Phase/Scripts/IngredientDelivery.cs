using UnityEngine;

public class IngredientDelivery : MonoBehaviour
{
    [SerializeField] private Ingredient ingredientPrefab;
    [SerializeField] private float spread = 0.6f;

    public void Deliver(IngredientData data)
    {
        Vector3 position = transform.position + Vector3.right * Random.Range(-spread, spread);
        Ingredient ingredient = Instantiate(ingredientPrefab, position, Quaternion.identity);
        ingredient.data = data;
        ingredient.name = data.displayName;
        ingredient.ApplyLook();
    }
}
