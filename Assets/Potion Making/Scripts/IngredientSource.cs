using UnityEngine;

// A shelf spot that holds one ingredient at a time: the next one appears a short delay after the last is used or gone.
public class IngredientSource : MonoBehaviour
{
    [SerializeField] private Ingredient ingredientPrefab;
    [SerializeField] private IngredientData ingredient;
    [SerializeField] private float restockDelay = 1f;

    private Ingredient current;
    private float timer;

    private void Start() => Restock();

    private void Update()
    {
        // carrying it around or leaving it somewhere still counts as out
        bool gone = current == null || current.Used;
        if (!gone)
        {
            timer = 0f;
            return;
        }
        timer += Time.deltaTime;
        if (timer >= restockDelay) Restock();
    }

    private void Restock()
    {
        timer = 0f;
        if (ingredientPrefab == null || ingredient == null) return;
        current = Instantiate(ingredientPrefab, transform.position, Quaternion.identity);
        current.data = ingredient;
        current.name = ingredient.displayName;
        current.ApplyLook();
    }
}
