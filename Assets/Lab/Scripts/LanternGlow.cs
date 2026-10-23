using UnityEngine;

public class LanternGlow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer glow;
    [SerializeField] private float minAlpha = 0.45f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float speed = 0.7f;
    [SerializeField] private float pulseSize = 0.08f;

    private float seed;
    private Vector3 baseScale;

    private void Awake()
    {
        if (glow == null) glow = GetComponentInChildren<SpriteRenderer>();
        seed = Random.value * 100f;
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (glow == null) return;
        float t = Mathf.PerlinNoise(seed, Time.time * speed);
        var color = glow.color;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        glow.color = color;
        transform.localScale = baseScale * (1f + (t - 0.5f) * pulseSize);
    }
}
