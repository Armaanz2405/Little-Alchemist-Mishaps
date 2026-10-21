using UnityEngine;

[CreateAssetMenu(fileName = "NewFamiliar", menuName = "Buy Phase/Familiar")]
public class FamiliarData : ScriptableObject
{
    public string displayName = "New Familiar";
    public Sprite sprite;
    public int price = 50;
    // 0.5 = jumps go 50% higher
    public float jumpHeightBonus = 0f;
}
