using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class DayNightVisuals : MonoBehaviour
{
    public Gradient lightColor;      // left = morning, right = night
    public AnimationCurve lightIntensity = AnimationCurve.Linear(0, 1, 1, 0.3f);

    [Tooltip("How dark the last NPC of the day gets (1 = full night)")]
    [Range(0f, 1f)] public float duskAmount = 0.8f;

    public float smoothSpeed = 1.5f;

    private Light2D globalLight;
    private float current;

    private void Awake()
    {
        globalLight = GetComponent<Light2D>();
    }

    private void Update()
    {
        var dm = DayManager.Instance;
        if (dm == null) return;

        float target = dm.Phase == DayPhase.Day
            ? dm.DayProgress01 * duskAmount
            : 1f;

        current = Mathf.MoveTowards(current, target, smoothSpeed * Time.unscaledDeltaTime);

        globalLight.color = lightColor.Evaluate(current);
        globalLight.intensity = lightIntensity.Evaluate(current);
    }
}