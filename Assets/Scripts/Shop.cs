using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] GameObject pedestalUI;

    public List<ShopItem> items = new List<ShopItem>();

    public Customer customer;

    public int numCustomersCame;
    public int numCustomersBought;

    //public bool isCustomerReady;
    public bool isShopOpen;

    bool spawningCustomer;

    float dayTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        dayTimer = 0;
    }

    // Update is called once per frame
    void Update() {
        if(isShopOpen) dayTimer += Time.deltaTime;

        // Basically, after the day timer is finished, we check the number of customers that came.
        // If the number of customers that came and bought is less than 3, keep the day going, UNLESS the
        // entire number of customers that came is over 20. This will let the day pass but ensure the player
        // is able to at least make a few sales. But if they aren't making sales and not changing anything,
        // the day should still end eventually (after 20 customers).
        if (dayTimer >= 21 && (numCustomersBought >= 3 || numCustomersCame >= 20)) {
            StopCoroutine(SpawnCustomer());
            isShopOpen = false;
        }
        if (isShopOpen && !IsCustomerShopping() && !spawningCustomer && items.Count > 0) {
            StartCoroutine(SpawnCustomer());
        }
    }

    IEnumerator SpawnCustomer() {
        spawningCustomer = true;
        yield return new WaitForSeconds(UnityEngine.Random.Range(3, 7));
        numCustomersCame++;
        customer.gameObject.SetActive(true);
        spawningCustomer = false;
    }

    public bool IsCustomerShopping() {
        return customer.gameObject.activeSelf;
    }

    public bool AddItemToShop(Pedestal _pedestal) {
        try {
            if (_pedestal.sellItem == null) return false;
            items.Add(new ShopItem(_pedestal.sellItem, _pedestal.sellPrice, _pedestal));
            return true;
        }
        catch {
            Debug.LogWarning("Unable to add item to shop with associating pedestal: " + _pedestal.name);
            return false;
        }
    }

    public bool ShopHasItem(Pedestal _pedestal) {
        return items.Any(x => x.pedestal == _pedestal);
    }

    public bool UpdateItem(Pedestal _pedestal) {
        try {
            RemoveItem(_pedestal);
            AddItemToShop(_pedestal);
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

[Serializable]
public struct ShopItem {
    public Item item;
    public int price;
    public Pedestal pedestal;

    public ShopItem(Item _item, int _price, Pedestal _pedestal) {
        item = _item;
        price = _price;
        pedestal = _pedestal;
    }
}