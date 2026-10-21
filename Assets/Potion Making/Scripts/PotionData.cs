using UnityEngine;

// One kind of potion (e.g. Springstep Draught). Create via Assets > Create > Potion Making > Potion.
[CreateAssetMenu(fileName = "NewPotion", menuName = "Potion Making/Potion")]
public class PotionData : ScriptableObject
{
    public string displayName = "New Potion";
    [Tooltip("Placeholder tint until we have art.")]
    public Color color = Color.white;
    [Tooltip("Optional. Leave empty to keep the prefab's sprite.")]
    public Sprite sprite;
}
