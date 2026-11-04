using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PotionFeedback : MonoBehaviour
{
    public static PotionFeedback Instance { get; private set; }

    [Header("Who the bubble appears next to")]
    public Transform npc;

    [Tooltip("0,0 = middle of the NPC. 1,1 = its top-right corner.")]
    public Vector2 anchorOnNpc = new Vector2(0.6f, 0.4f);

    [Tooltip("Extra nudge in pixels")]
    public Vector2 pixelOffset = new Vector2(0f, 0f);

    public float screenMargin = 30f;

    [Header("Messages (one is picked at random)")]
    public string[] goodMessages = { "Well done!" };
    public string[] badMessages = { "Hmm..." };

    [Header("Bubble colors")]
    public Color goodColor = new Color(0.25f, 0.55f, 0.3f, 0.9f);
    public Color badColor = new Color(0.55f, 0.35f, 0.25f, 0.9f);

    [Header("Random mode")]
    [Range(0f, 1f)] public float goodChance = 0.5f;

    [Header("Timing")]
    public float fadeIn = 0.3f;
    public float hold = 1.5f;
    public float fadeOut = 0.5f;

    [Header("Testing")]
    [Tooltip("G = well done, H = hmm, R = random. Untick when the real potion code is hooked up.")]
    public bool enableTestKeys = true;

    private CanvasGroup group;
    private TMP_Text label;
    private Image background;
    private RectTransform rect;
    private Canvas canvas;
    private Camera cam;
    private Transform currentTarget;
    private Coroutine running;

    public float TotalDuration
    {
        get { return fadeIn + hold + fadeOut; }
    }

    void Awake()
    {
        Instance = this;

        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        background = GetComponent<Image>();
        label = GetComponentInChildren<TMP_Text>(true);

        group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }

        if (label == null)
        {
            Debug.LogError("PotionFeedback: put this script on FeedbackBubble, with FeedbackText inside it.");
            return;
        }

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    void Update()
    {
        if (!enableTestKeys || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Show(true);
        }
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            Show(false);
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ShowRandom();
        }
    }

    void LateUpdate()
    {
        if (group != null && group.alpha > 0f)
        {
            PositionBubble();
        }
    }

    // ---------- things other scripts can call ----------

    public void Show(bool wasGood)
    {
        Show(wasGood, npc);
    }

    public void Show(bool wasGood, Transform target)
    {
        if (label == null)
        {
            return;
        }

        currentTarget = target != null ? target : npc;

        string[] pool = wasGood ? goodMessages : badMessages;
        label.text = pool.Length > 0 ? pool[Random.Range(0, pool.Length)] : "";

        if (background != null)
        {
            background.color = wasGood ? goodColor : badColor;
        }

        if (running != null)
        {
            StopCoroutine(running);
        }
        running = StartCoroutine(Sequence());
    }

    public void ShowRandom()
    {
        Show(Random.value < goodChance);
    }

    // Shows the message, THEN counts the NPC as helped for the day
    public void ShowAndHelpNpc(bool wasGood)
    {
        Show(wasGood);
        StartCoroutine(CountNpcAfterMessage());
    }

    public void ShowRandomAndHelpNpc()
    {
        ShowAndHelpNpc(Random.value < goodChance);
    }

    // ---------- internals ----------

    IEnumerator CountNpcAfterMessage()
    {
        yield return new WaitForSecondsRealtime(TotalDuration);

        if (DayManager.Instance != null)
        {
            DayManager.Instance.NpcHelped();
        }
    }

    void PositionBubble()
    {
        if (currentTarget == null || rect == null)
        {
            return;
        }

        if (cam == null)
        {
            cam = Camera.main;
        }
        if (cam == null)
        {
            return;
        }

        Vector3 worldPoint = currentTarget.position;
        Renderer r = currentTarget.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            Bounds b = r.bounds;
            worldPoint = b.center + new Vector3(b.extents.x * anchorOnNpc.x, b.extents.y * anchorOnNpc.y, 0f);
        }

        float scale = canvas != null ? canvas.scaleFactor : 1f;

        Vector3 screen = cam.WorldToScreenPoint(worldPoint);
        screen.x += pixelOffset.x * scale;
        screen.y += pixelOffset.y * scale;

        // keep the bubble inside the visible game area
        Rect view = cam.pixelRect;
        float halfW = rect.rect.width * 0.5f * scale + screenMargin;
        float halfH = rect.rect.height * 0.5f * scale + screenMargin;
        screen.x = Mathf.Clamp(screen.x, view.xMin + halfW, view.xMax - halfW);
        screen.y = Mathf.Clamp(screen.y, view.yMin + halfH, view.yMax - halfH);

        rect.position = new Vector3(screen.x, screen.y, 0f);
    }

    IEnumerator Sequence()
    {
        Vector3 big = Vector3.one;
        Vector3 small = Vector3.one * 0.8f;

        PositionBubble();

        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeIn);
            group.alpha = p;
            rect.localScale = Vector3.Lerp(small, big, p);
            yield return null;
        }
        group.alpha = 1f;
        rect.localScale = big;

        yield return new WaitForSecondsRealtime(hold);

        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.Clamp01(t / fadeOut);
            yield return null;
        }
        group.alpha = 0f;
    }
}