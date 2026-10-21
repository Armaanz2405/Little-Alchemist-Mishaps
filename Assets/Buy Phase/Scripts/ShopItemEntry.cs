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
    [SerializeField] private TMP_Text buyLabel;

    public int Price { get; private set; }
    public bool Owned { get; private set; }
    public Button BuyButton => buyButton;

    public void Show(string displayName, Sprite sprite, Color color, int price, Action onBuy)
    {
        Price = price;
        if (sprite != null)
        {
            icon.sprite = sprite;
            icon.color = Color.white;
            icon.preserveAspect = true;
        }
        else
        {
            icon.color = color;
        }
        nameText.text = displayName;
        priceText.text = price + " gold";
        buyButton.onClick.AddListener(() => onBuy());
    }

    public void SetAffordable(bool canAfford)
    {
        buyButton.interactable = !Owned && canAfford;
    }

    public void MarkOwned()
    {
        Owned = true;
        buyButton.interactable = false;
        if (buyLabel != null)
        {
            buyLabel.text = "Owned";
        }
    }
}
