using UnityEngine;

// mortar prompt with how many ingredients are in it
[RequireComponent(typeof(MortarNPestle))]
public class MortarPrompt : StationPrompt
{
    private MortarNPestle mortar;

    protected override void Awake()
    {
        base.Awake();
        mortar = GetComponent<MortarNPestle>();
    }

    protected override string Counter() => $"{mortar.Contents.Count}/{MortarNPestle.MaxIngredientsPerCrush}";
}
