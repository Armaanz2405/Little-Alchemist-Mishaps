using UnityEngine;

// slides side to side on its own whenever nobody is carrying it
public class WanderHazard : IngredientHazard
{
    // how far it goes each way from where it was put down
    [SerializeField] private float distance = 1f;
    // top speed in units per second
    [SerializeField] private float speed = 1.5f;

    private float time;

    protected override void OnLetGo(GrabAndThrow by) => time = 0f;

    protected override void OnFixedStep()
    {
        if (IsHeld || distance <= 0f) return;
        time += Time.fixedDeltaTime;
        float rate = speed / distance;
        var v = Body.linearVelocity;
        Body.linearVelocity = new Vector2(Mathf.Cos(time * rate) * speed, v.y);
    }
}
