using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct ShopItem {
    public Item item;
    public float price;
    public Pedestal pedestal;

    public ShopItem(Item _item, float _price, Pedestal _pedestal) {
        item = _item;
        price = _price;
        pedestal = _pedestal;
    }
}

public class Shop : MonoBehaviour
{
    [SerializeField] GameObject pedestalUI;

    public List<ShopItem> items = new List<ShopItem>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool AddItem(Pedestal _pedestal) {
        try {
            items.Add(new ShopItem(_pedestal.sellItem, _pedestal.sellPrice, _pedestal));
            return true;
        }
        catch {
            Debug.LogWarning("Unable to add item to shop with associating pedestal: " + _pedestal.name);
            return false;
        }
    }

    public bool UpdateItem(Pedestal _pedestal) {
        try {
            RemoveItem(_pedestal);
            AddItem(_pedestal);
            return true;
        }
        catch {
            Debug.LogWarning("Unable to add item to shop with associating pedestal: " + _pedestal.name);
            return false;
        }
    }

    public bool RemoveItem(Pedestal _pedestal) {
        try {
            if(items.Any(x => x.pedestal == _pedestal)) items.Remove(items.FirstOrDefault(x => x.pedestal == _pedestal));
            return true;
        }
        catch {
            Debug.LogWarning("Unable to remove item to shop with associating pedestal: " + _pedestal.name);
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<ShopItem> GetShopItems(bool onlyUniques = true) {
        if (onlyUniques) return items.Distinct().ToList();
        else return items;
    }

    public List<Item> GetItemsInShopItems(bool onlyUniques = true) {
        List<Item> _items = new List<Item>();
        foreach (ShopItem item in items) {
            _items.Add(item.item);
        }

        if (onlyUniques) return _items.Distinct().ToList();
        else return _items;
    }
}
