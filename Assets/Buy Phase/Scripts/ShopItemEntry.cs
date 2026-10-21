using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemEntry : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    public ShopCatalog.Item Item { get; private set; }
    public Button BuyButton => buyButton;

    public void Show(ShopCatalog.Item item, Action<ShopCatalog.Item> onBuy)
    {
        Item = item;
        IngredientData ingredient = item.ingredient;
        icon.color = ingredient.color;
        if (ingredient.sprite != null)
        {
            icon.sprite = ingredient.sprite;
        }
        nameText.text = ingredient.displayName;
        priceText.text = item.price + " gold";
        buyButton.onClick.AddListener(() => onBuy(item));
    }

    public void SetAffordable(bool canAfford)
    {
        buyButton.interactable = canAfford;
    }
}
