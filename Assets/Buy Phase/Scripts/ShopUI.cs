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

    [SerializeField] private ShopCatalog ingredientCatalog;
    [SerializeField] private Transform ingredientGrid;
    [SerializeField] private ShopItemEntry entryPrefab;

    [SerializeField] private Wallet wallet;
    [SerializeField] private IngredientDelivery delivery;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private InputAction toggleAction = DefaultToggleAction();

    private readonly List<ShopItemEntry> entries = new List<ShopItemEntry>();
    private int currentTab;

    public bool IsOpen => window.activeSelf;

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

        if (ingredientCatalog == null)
        {
            Debug.LogWarning("[Shop] No ingredient catalog assigned, so the Ingredients tab is empty.", this);
            return;
        }

        foreach (ShopCatalog.Item item in ingredientCatalog.items)
        {
            if (item.ingredient == null)
            {
                continue;
            }
            ShopItemEntry entry = Instantiate(entryPrefab, ingredientGrid);
            entry.Show(item, Buy);
            entries.Add(entry);
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
        GameObject first = tab == 0 && entries.Count > 0 ? entries[0].BuyButton.gameObject : tabButtons[tab].gameObject;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(first);
        }
    }

    private void Buy(ShopCatalog.Item item)
    {
        if (!wallet.TrySpend(item.price))
        {
            return;
        }
        delivery.Deliver(item.ingredient);
        Refresh();
    }

    private void Refresh()
    {
        goldText.text = wallet.Unlimited ? "Gold: Unlimited" : "Gold: " + wallet.Gold;
        foreach (ShopItemEntry entry in entries)
        {
            entry.SetAffordable(wallet.CanAfford(entry.Item.price));
        }
    }
}
