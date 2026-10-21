using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] private bool unlimited = true;
    [SerializeField] private int gold = 50;

    public event Action Changed;

    public bool Unlimited => unlimited;
    public int Gold => gold;

    public bool CanAfford(int price)
    {
        return unlimited || gold >= price;
    }

    public bool TrySpend(int price)
    {
        if (!CanAfford(price))
        {
            return false;
        }
        if (!unlimited)
        {
            gold -= price;
            Changed?.Invoke();
        }
        return true;
    }

    public void Add(int amount)
    {
        gold += amount;
        Changed?.Invoke();
    }
}
