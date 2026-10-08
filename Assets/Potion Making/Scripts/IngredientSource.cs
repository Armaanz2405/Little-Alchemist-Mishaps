using UnityEngine;

// A shelf spot that always has an ingredient on it: take it and a new one appears after a short delay.
public class IngredientSource : MonoBehaviour
{
    [SerializeField] private Ingredient ingredientPrefab;
    [SerializeField] private IngredientData ingredient;
    [SerializeField] private float restockDelay = 1f;
    [Tooltip("How far the ingredient has to move away before it counts as taken.")]
    [SerializeField] private float takenDistance = 0.5f;

    private Ingredient current;
    private float timer;

    private void Start() => Restock();

    private void Update()
    {
        bool taken = current == null || (current.transform.position - transform.position).sqrMagnitude > takenDistance * takenDistance;
        if (!taken)
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
