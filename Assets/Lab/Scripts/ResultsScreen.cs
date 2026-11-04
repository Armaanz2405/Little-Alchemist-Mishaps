using Nocturne.PotionMaking.Energy;
using Nocturne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// press ' to see how the player did
public class ResultsScreen : MonoBehaviour
{
    [SerializeField] private Key toggleKey = Key.Quote;
    [SerializeField] private PlayerInput playerInput;

    private GameObject window;
    private float previousTimeScale = 1f;

    void Start()
    {
        if (playerInput == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerInput = player.GetComponent<PlayerInput>();
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (window != null && window.activeSelf) Close();
            // not while talking or in another menu
            else if (playerInput == null || playerInput.inputIsActive) Open();
        }
    }

    private void Open()
    {
        if (window != null) Destroy(window);
        Build();
        previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
        if (playerInput != null) playerInput.DeactivateInput();
    }

    private void Close()
    {
        window.SetActive(false);
        Time.timeScale = previousTimeScale;
        if (playerInput != null) playerInput.ActivateInput();
    }

    private void Build()
    {
        var canvas = UIStyle.MakeCanvas(transform, "Results", 94);
        window = canvas.gameObject;

        var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        dim.transform.SetParent(canvas.transform, false);
        var drt = (RectTransform)dim.transform;
        drt.anchorMin = Vector2.zero;
        drt.anchorMax = Vector2.one;
        drt.offsetMin = drt.offsetMax = Vector2.zero;
        dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panel.transform.SetParent(canvas.transform, false);
        var rt = (RectTransform)panel.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        UIStyle.Box(panel.GetComponent<Image>(), BoxKind.Panel);
        var v = panel.GetComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(56, 56, 40, 44);
        v.spacing = 14;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = v.childForceExpandHeight = false;
        var fit = panel.GetComponent<ContentSizeFitter>();
        fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Centered(UIStyle.MakeText(panel.transform, "Results", 54, TextKind.Title, "Title"));
        UIStyle.Divider(panel.transform, 14f).GetComponent<LayoutElement>().minWidth = 560f;

        var customer = FindAnyObjectByType<Customer_Order>();
        if (customer == null) return;

        Row(panel.transform, TextKind.Muted, "Order", "Wanted", "Potion", "Minigame score", "Energy left");
        for (int i = 0; i < customer.Submissions.Count; i++)
        {
            var s = customer.Submissions[i];
            Row(panel.transform, TextKind.Body, (i + 1).ToString(), s.order, s.potionTags,
                MinigameScore(s.minigameScoreTotal, s.minigamesPlayed), $"{s.energyLeft} / {s.energyMax}");
        }
        var meter = FindAnyObjectByType<EnergyMeter>();
        Row(panel.transform, TextKind.Muted, (customer.Submissions.Count + 1).ToString(), customer.OrderText, "in progress",
            MinigameScore(customer.MinigameScoreTotal, customer.MinigamesPlayed), meter != null ? $"{meter.Current} / {meter.Max}" : "");
    }

    private static string MinigameScore(float total, int played) =>
        played > 0 ? $"{Mathf.RoundToInt(100f * total / played)}%" : "none played";

    private static readonly float[] ColumnWidths = { 80f, 300f, 360f, 220f, 150f };

    private static void Row(Transform parent, TextKind kind, params string[] cells)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);
        var h = row.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 12;
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = false;
        for (int i = 0; i < cells.Length; i++)
        {
            var text = UIStyle.MakeText(row.transform, cells[i], 24, kind, "Cell");
            var le = text.gameObject.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = ColumnWidths[i];
        }
    }

    private static void Centered(TextMeshProUGUI text) => text.horizontalAlignment = HorizontalAlignmentOptions.Center;
}
