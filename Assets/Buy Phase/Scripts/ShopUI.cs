using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private GameObject[] tabPages;
    [SerializeField] private Color tabColor = new Color(0.25f, 0.21f, 0.29f);
    [SerializeField] private Color selectedTabColor = new Color(0.55f, 0.42f, 0.62f);
    [SerializeField] private ShopItemEntry entryPrefab;

    [SerializeField] private ShopCatalog ingredientCatalog;
    [SerializeField] private Transform ingredientGrid;
    [SerializeField] private IngredientDelivery delivery;

    [SerializeField] private FamiliarData[] familiars;
    [SerializeField] private Transform familiarGrid;
    [SerializeField] private FamiliarPerch familiarPerch;

    [SerializeField] private Wallet wallet;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private InputAction toggleAction = DefaultToggleAction();

    private readonly List<ShopItemEntry> ingredientEntries = new List<ShopItemEntry>();
    private readonly List<ShopItemEntry> familiarEntries = new List<ShopItemEntry>();
    private int currentTab;

    public bool IsOpen => window.activeSelf;
    public event Action<bool> OpenChanged;

    private static InputAction DefaultToggleAction()
    {
        InputAction action = new InputAction("Toggle Shop", InputActionType.Button, "<Keyboard>/b");
        action.AddBinding("<Gamepad>/select");
        return action;
    }

    private void Awake()
    {
        window.SetActive(false);

        for (int i = 0; i < tabButtons.Length; i++)
        {
            int tab = i;
            tabButtons[i].onClick.AddListener(() => ShowTab(tab));
        }

        if (ingredientCatalog != null)
        {
            foreach (ShopCatalog.Item item in ingredientCatalog.items)
            {
                if (item.ingredient == null)
                {
                    continue;
                }
                ShopItemEntry entry = Instantiate(entryPrefab, ingredientGrid);
                IngredientData ingredient = item.ingredient;
                entry.Show(ingredient.displayName, ingredient.sprite, ingredient.color, item.price, () => BuyIngredient(item));
                ingredientEntries.Add(entry);
            }
        }
        else
        {
            Debug.LogWarning("[Shop] No ingredient catalog assigned, so the Ingredients tab is empty.", this);
        }

        if (familiarGrid != null)
        {
            foreach (FamiliarData familiar in familiars)
            {
                if (familiar == null)
                {
                    continue;
                }
                ShopItemEntry entry = Instantiate(entryPrefab, familiarGrid);
                entry.Show(familiar.displayName, familiar.sprite, Color.white, familiar.price, () => BuyFamiliar(familiar, entry));
                familiarEntries.Add(entry);
            }
        }
    }

    private void OnEnable()
    {
        toggleAction.performed += OnToggle;
        toggleAction.Enable();
        if (wallet != null)
        {
            wallet.Changed += Refresh;
        }
    }

    private void OnDisable()
    {
        toggleAction.performed -= OnToggle;
        toggleAction.Disable();
        if (wallet != null)
        {
            wallet.Changed -= Refresh;
        }
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        SetOpen(!IsOpen);
    }

    public void SetOpen(bool open)
    {
        window.SetActive(open);

        // stop the player walking around while the shop is open
        if (playerInput != null)
        {
            if (open)
            {
                playerInput.DeactivateInput();
            }
            else
            {
                playerInput.ActivateInput();
            }
        }

        if (open)
        {
            ShowTab(currentTab);
            Refresh();
        }

        OpenChanged?.Invoke(open);
    }

    public void ShowTab(int tab)
    {
        currentTab = tab;
        for (int i = 0; i < tabPages.Length; i++)
        {
            tabPages[i].SetActive(i == tab);
            tabButtons[i].image.color = i == tab ? selectedTabColor : tabColor;
        }

        // give controllers something selected to start from
        List<ShopItemEntry> entries = tab == 0 ? ingredientEntries : tab == 1 ? familiarEntries : null;
        ShopItemEntry first = entries?.Find(entry => !entry.Owned);
        GameObject selected = first != null ? first.BuyButton.gameObject : tabButtons[tab].gameObject;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(selected);
        }
    }

    private void BuyIngredient(ShopCatalog.Item item)
    {
        if (!wallet.TrySpend(item.price))
        {
            return;
        }
        delivery.Deliver(item.ingredient);
        Refresh();
    }

    private void BuyFamiliar(FamiliarData familiar, ShopItemEntry entry)
    {
        if (familiarPerch.Owns(familiar) || !wallet.TrySpend(familiar.price))
        {
            return;
        }
        familiarPerch.Add(familiar);
        entry.MarkOwned();
        Refresh();
        ShowTab(currentTab);
    }

    private void Refresh()
    {
        goldText.text = wallet.Unlimited ? "Gold: Unlimited" : "Gold: " + wallet.Gold;
        foreach (ShopItemEntry entry in ingredientEntries)
        {
            entry.SetAffordable(wallet.CanAfford(entry.Price));
        }
        foreach (ShopItemEntry entry in familiarEntries)
        {
            entry.SetAffordable(wallet.CanAfford(entry.Price));
        }
    }
}
