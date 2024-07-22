using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Customer : MonoBehaviour
{
    Shop shop;

    public enum CustomerStates {
        Browsing,
        Buying,
        Leaving
    }
    public CustomerStates currentState;

    public enum PurchasingState {
        WillBuy,
        TooExpensive,
        WontBuy
    }
    public PurchasingState customerPurchasingState;

    public LootTable desiredItemsPool;
    List<Item> desiredItems;
    ShopItem itemToBuy;

    public int itemsLookedAt;

    public int money;

    bool shopHasItem;

    /**
     * just gonna jot some ideas down for all of this
     * 
     * just like in moonlighter, the player should be able to open the shop. the "day" won't start until they do so, and this will allow them 
     * to make/change wares before customers come. in terms of like a "time" system for when customers stop coming, this will probably be based 
     * on the number of customers that came and not on actual time. this way, it is less RNG based and the player won't ever make no money 
     * because no customers came in the alloted time.
     * 
     * will probably try to use navmesh to make the roaming. can't do that until we get colliders working for the tileset
     * 
     * when a customer comes in and the shop has some of the items they want, they will pick one (maybe multiple) items from their
     * list to buy. if the shop DOESNT have any of the items they want, they will do one of two things:
     *  a) they will pick an item from the shop to buy. (maybe make them be more picky about the price, but that feature could get kinda unpredictable)
     *  b) they will roam for a bit and then leave, but before they leave they will have a bubble appear that shows the item they actually want to buy
     * 
     * money for the time being will just be a random number that is generated. the generated numbers will be able to be higher as the player progresses
     * 
     * there should probably only be a certain amount of customers in the store at once
    **/

    // Start is called before the first frame update
    void Start()
    {
        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();

        money = Random.Range(20, 100);

        CheckForItems();
    }

    void CheckForItems() {
        // The customer is going to roll their desiredItemsPool to get the items they want to buy
        desiredItems = desiredItemsPool.RollDrops().Distinct().ToList();

        // If there are no items in shop, just go straight to rolling from desiredItems
        if (shop.GetShopItems().Count <= 0) {
            customerPurchasingState = PurchasingState.WontBuy;
            itemToBuy = new ShopItem(desiredItems[Random.Range(0, desiredItems.Count)], -1, null);
        }
        // Evaluate items in the shop
        else {
            shopHasItem = false; // Initialize this to false

            // We check if the shop has any of the items the customer wants to buy, and if so, adjust desiredItems accordingly.
            if (desiredItems.Intersect(shop.GetItemsInShopItems()).Any()) {
                shopHasItem = true;
                desiredItems = desiredItems.Intersect(shop.GetItemsInShopItems()).ToList();
            }

            // If the shop DOES have an item that the customer wants to buy
            if (shopHasItem) {
                customerPurchasingState = PurchasingState.WillBuy;
                itemToBuy = shop.GetShopItems().First(x => x.item == desiredItems[Random.Range(0, desiredItems.Count)]);

                // If the item is priced too high
                if (itemToBuy.price > itemToBuy.item.marketPrice) customerPurchasingState = PurchasingState.TooExpensive;
            }
            // If the shop DOES NOT have an item that the customer wants to buy
            else {
                // Randomize whether the customer will buy an available item or not

                // Customer WILL NOT buy an item. itemToBuy is set to an item they want to buy, to inform the player.
                if (Random.Range(0, 2) == 0) { // 50% chance
                    customerPurchasingState = PurchasingState.WontBuy;
                    itemToBuy = new ShopItem(desiredItems[Random.Range(0, desiredItems.Count)], -1, null);
                }
                // Customer WILL buy an item. itemToBuy is set to one of the items offered in the shop
                else {
                    customerPurchasingState = PurchasingState.WillBuy;
                    itemToBuy = shop.GetShopItems().ToList()[Random.Range(0, shop.GetShopItems().Count)];

                    // If the item is priced too high
                    if (itemToBuy.price > itemToBuy.item.marketPrice) customerPurchasingState = PurchasingState.TooExpensive;
                }
            }
        }

        switch(customerPurchasingState) {
            case PurchasingState.WillBuy:
                Debug.Log("Customer will make a purchase! They want " + itemToBuy.item.name + " which is priced at $" + itemToBuy.price + " and is in pedestal: " + itemToBuy.pedestal.name + ".");
                break;
            case PurchasingState.TooExpensive:
                Debug.Log("Customer would have bought for a more reasonable price! They wanted " + itemToBuy.item.name + " which is priced at $" + itemToBuy.price + " (compared to market value: $" + itemToBuy.item.marketPrice + "), and is in pedestal: " + itemToBuy.pedestal.name + ".");
                break;
            case PurchasingState.WontBuy:
                Debug.Log("Customer will not make a purchase. They want to buy " + itemToBuy.item.name + ".");
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) CheckForItems();

        switch (currentState) {
            case CustomerStates.Browsing:
                break;
            case CustomerStates.Buying:
                break;
            case CustomerStates.Leaving:
                break;
            default:
                break;
        }
    }
}
