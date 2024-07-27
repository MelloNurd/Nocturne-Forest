using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

public class Customer : MonoBehaviour
{
    Shop shop;

    GameObject playerObj;

    Rigidbody2D rb;

    [SerializeField] ShopCheckout checkout;

    [SerializeField] Animator animator;
    public int numTypesCustomers = 1;

    public enum CustomerStates {
        Thinking, // This is basically just idle
        Browsing,
        Buying,
        ReadyToPurchase,
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

    bool shopHasItem;

    int walkCycles;
    int maxWalkCycles;

    List<GameObject> pedestals = new List<GameObject>();

    public float moveSpeed = 0.04f;
    Vector3 currDestination;
    Vector3 walkDir;

    Vector3 doorPos;
    Vector3 buyPos = new Vector3(2.5f, 0, 0);
    Vector3 buyLinePos = new Vector3(2.5f, -1.6f, 0);

    SpriteRenderer itemSpriteRenderer;
    SpriteRenderer bubbleSpriteRenderer;
    [SerializeField] Sprite thinkingSprite;
    [SerializeField] Sprite emptyBubbleSprite;
    [SerializeField] Sprite tooExpensiveSprite;

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
    void Awake()
    {
        itemSpriteRenderer = transform.Find("Item").GetComponent<SpriteRenderer>();
        bubbleSpriteRenderer = transform.Find("SpeechBubble").GetComponent<SpriteRenderer>();

        shop = GameObject.FindGameObjectWithTag("Shop").GetComponent<Shop>();
        playerObj = GameObject.FindGameObjectWithTag("Player");

        rb = GetComponent<Rigidbody2D>();

        doorPos = transform.position;
    }

    private void OnEnable() {
        animator.SetInteger("PersonType", Random.Range(1, numTypesCustomers+1));
        currentState = CustomerStates.Thinking;
        StartCoroutine(StartBrowsing());

        walkCycles = 0;
        maxWalkCycles = Random.Range(2, 5);

        itemSpriteRenderer.sortingOrder = -1;
        itemSpriteRenderer.transform.localPosition = new Vector2(0, 0.4f);
        itemSpriteRenderer.sprite = null;
        bubbleSpriteRenderer.sprite = null;
        bubbleSpriteRenderer.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
    }

    IEnumerator StartBrowsing() {
        yield return new WaitForSeconds(Random.Range(0.6f, 2f));
        pedestals = GameObject.FindGameObjectsWithTag("Pedestal").ToList().FindAll(x => x.GetComponent<Pedestal>().sellItem != null);
        CheckForItems();
        if (pedestals.Count <= 0) {
            currDestination = doorPos;
            currentState = CustomerStates.Leaving;
        }
        else {
            currDestination = GetNewPos();
            currentState = CustomerStates.Browsing;
        }
    }

    private void Update() {
        walkDir = (currDestination - transform.position).normalized;
        if(currentState != CustomerStates.Thinking)
        {
            animator.SetFloat("XInput", walkDir.x);
            animator.SetFloat("YInput", walkDir.y);
        }

    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (currentState) {
            case CustomerStates.Thinking:
                break;
            case CustomerStates.Browsing:
                transform.position += walkDir * moveSpeed;
                if (Vector3.Distance(transform.position, currDestination) < 0.1f) {
                    if(walkCycles < maxWalkCycles) {
                        StartCoroutine(SetDestination());
                    }
                    else {
                        if (customerPurchasingState == PurchasingState.WillBuy) {
                            currDestination = buyLinePos;
                            currentState = CustomerStates.Buying;
                            itemSpriteRenderer.sprite = itemToBuy.item.image;
                            itemToBuy.pedestal.ClearItem(true);
                            // GRAB ITEM
                        }
                        else {
                            currDestination = doorPos;
                            currentState = CustomerStates.Leaving;
                            bubbleSpriteRenderer.sprite = emptyBubbleSprite;
                            if (customerPurchasingState == PurchasingState.TooExpensive) {
                                bubbleSpriteRenderer.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = tooExpensiveSprite;
                            }
                            else if (customerPurchasingState == PurchasingState.WontBuy) {
                                bubbleSpriteRenderer.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = itemToBuy.item.image;
                            }
                        }
                    }
                }
                break;
            case CustomerStates.Buying:
                transform.position += walkDir * moveSpeed;
                if (Vector3.Distance(transform.position, currDestination) < 0.1f) {
                    if (currDestination == buyLinePos) currDestination = buyPos;
                    else {
                        currentState = CustomerStates.ReadyToPurchase;
                        checkout.canInteract = true;
                    }
                }
                break;
            case CustomerStates.ReadyToPurchase:
                break;
            case CustomerStates.Leaving:
                transform.position += walkDir * moveSpeed;
                if (Vector3.Distance(transform.position, currDestination) < 0.1f) {
                    if (currDestination == buyLinePos) currDestination = doorPos;
                    else {
                        animator.SetTrigger("Reset");
                        animator.ResetTrigger("Reset");
                        gameObject.SetActive(false);
                    }
                }
                break;
            default:
                break;
        }
    }

    IEnumerator SetDestination() {
        currentState = CustomerStates.Thinking;
        Vector3 newPos = GetNewPos();

        if (walkCycles + 1 >= maxWalkCycles && (customerPurchasingState == PurchasingState.WillBuy || customerPurchasingState == PurchasingState.TooExpensive)) {
            newPos = itemToBuy.pedestal.transform.position + Vector3.down;
        }
        else if (pedestals.Count > 1) {
            int cutoff = 0; // Only doing this in case of infinite loops. Had a few during testing but shouldn't happen anymore, leaving just in case.
            while(currDestination == newPos && cutoff < 50) {
                newPos = GetNewPos();
                cutoff++;
            }
        }
        else {
            walkCycles += maxWalkCycles; // If there is only one pedestal, customer doesn't need to browse...
        }
        animator.SetFloat("XInput", 0f);
        animator.SetFloat("YInput", 1f);
        StartCoroutine(ThinkIcon());
        yield return new WaitForSeconds(Random.Range(2.5f, 5f));
        currentState = CustomerStates.Browsing;
        currDestination = newPos;
        walkCycles++;
    }

    Vector3 GetNewPos() {
        return pedestals[Random.Range(0, pedestals.Count)].transform.position + Vector3.down;
    }

    IEnumerator ThinkIcon() {
        bubbleSpriteRenderer.sprite = thinkingSprite;
        yield return new WaitForSeconds(0.8f);
        bubbleSpriteRenderer.sprite = null;
    }

    public void OnCheckout() {
        checkout.canInteract = false;
        checkout.DisableOutline();
        itemSpriteRenderer.sortingOrder = 2;
        itemSpriteRenderer.transform.localPosition = new Vector2(0, -itemSpriteRenderer.transform.position.y);
        currDestination = buyLinePos;
        currentState = CustomerStates.Leaving;
        InventoryManager.currentInstance.playerCash += itemToBuy.price;
        shop.numCustomersBought++;
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
                if (itemToBuy.price > itemToBuy.item.marketPrice * itemToBuy.pedestal.count) customerPurchasingState = PurchasingState.TooExpensive;
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
}
