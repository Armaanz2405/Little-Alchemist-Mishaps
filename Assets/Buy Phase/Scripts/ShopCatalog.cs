using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopCatalog", menuName = "Buy Phase/Shop Catalog")]
public class ShopCatalog : ScriptableObject
{
    [Serializable]
    public class Item
    {
        public IngredientData ingredient;
        public int price = 10;
    }

    public List<Item> items = new List<Item>();
}
