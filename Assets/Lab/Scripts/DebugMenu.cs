using System;
using Nocturne.PotionMaking.Energy;
using Nocturne.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// press ; to bring up
public class DebugMenu : MonoBehaviour
{
    [Header("Settings Asset")]
    [SerializeField] private EnergyCosts energyCosts;
    [SerializeField] private CustomerEnergy customerEnergy;
    [SerializeField] private Key toggleKey = Key.Semicolon;

    private GameObject window;

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (window != null && window.activeSelf) window.SetActive(false);
            else Open();
        }
    }

    private void Open()
    {
        if (energyCosts == null || customerEnergy == null)
        {
            Debug.LogWarning("[Debug Menu] Assign the Energy Costs and Customer Energy assets.", this);
            return;
        }
        if (window != null) Destroy(window);
        Build();
    }

    private void Build()
    {
        var canvas = UIStyle.MakeCanvas(transform, "Debug Menu", 95);
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        window = canvas.gameObject;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panel.transform.SetParent(canvas.transform, false);
        var rt = (RectTransform)panel.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(40f, -40f);
        UIStyle.Box(panel.GetComponent<Image>(), BoxKind.Panel, 1.3f);
        var v = panel.GetComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(32, 32, 24, 28);
        v.spacing = 12;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = v.childForceExpandHeight = false;
        var fit = panel.GetComponent<ContentSizeFitter>();
        fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        UIStyle.MakeText(panel.transform, "Debug Menu", 34, TextKind.Title, "Title");
        UIStyle.MakeText(panel.transform, "; to close", 20, TextKind.Muted, "Hint");
        UIStyle.Divider(panel.transform, 12f);

        var columns = new GameObject("Columns", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        columns.transform.SetParent(panel.transform, false);
        var h = columns.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 40;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = false;

        var costs = Column(columns.transform, "Energy Costs");
        var inspect = SliderRow(costs, "Inspect", 0, 3, energyCosts.inspect, x => energyCosts.inspect = x);
        var refine = SliderRow(costs, "Refine", 0, 3, energyCosts.refine, x => energyCosts.refine = x);
        var bottle = SliderRow(costs, "Bottle", 0, 3, energyCosts.bottle, x => energyCosts.bottle = x);
        var talk = SliderRow(costs, "Talk", 0, 3, energyCosts.talk, x => energyCosts.talk = x);
        UIStyle.MakeButton(costs, "Reset to Defaults", 260f, 48f, 22f).onClick.AddListener(() =>
        {
            inspect.value = refine.value = bottle.value = talk.value = 1;
        });

        var customers = Column(columns.transform, "Customer Energy");
        var standard = SliderRow(customers, "Standard Energy", 0, 6, customerEnergy.standardEnergy, x => customerEnergy.standardEnergy = x);
        var npcSliders = new Slider[customerEnergy.keyNpcs.Count];
        for (int i = 0; i < customerEnergy.keyNpcs.Count; i++)
        {
            var npc = customerEnergy.keyNpcs[i];
            npcSliders[i] = SliderRow(customers, npc.npcName, 0, 10, npc.energy, x => npc.energy = x);
        }
        UIStyle.MakeButton(customers, "Reset to Defaults", 260f, 48f, 22f).onClick.AddListener(() =>
        {
            standard.value = 5;
            foreach (var slider in npcSliders) slider.value = 6;
        });
    }

    private static Transform Column(Transform parent, string title)
    {
        var go = new GameObject(title, typeof(RectTransform), typeof(VerticalLayoutGroup));
        go.transform.SetParent(parent, false);
        var v = go.GetComponent<VerticalLayoutGroup>();
        v.spacing = 8;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = v.childForceExpandHeight = false;
        UIStyle.MakeText(go.transform, title, 26, TextKind.Accent, "Header");
        return go.transform;
    }

    private static Slider SliderRow(Transform parent, string label, int min, int max, int value, Action<int> onChange)
    {
        var text = UIStyle.MakeText(parent, "", 22, TextKind.Body, label);
        var slider = UIStyle.Slider(parent, min, max, value);
        void Show(float x) => text.text = $"{label}: <color={UITheme.Hex(UITheme.Current.goldText)}>{Mathf.RoundToInt(x)}</color>";
        Show(value);
        slider.onValueChanged.AddListener(x =>
        {
            onChange(Mathf.RoundToInt(x));
            Show(x);
        });
        return slider;
    }
}
