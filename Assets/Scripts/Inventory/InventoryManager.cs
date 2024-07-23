using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public enum InventoryOpening {
        PlayerInventory,
        ShopInventory,
        ItemPedestal,
        PotionCrafting,
        Closing
    }

    public static InventoryManager currentInstance; // There should only ever be one InventoryManager per scene, so we are doing this for easy access.

    public List<Recipe> globalCraftingRecipes = new List<Recipe>();

    int numHotbarSlots;
    [SerializeField] GameObject hotbarCover; // This is just to block mouse dragging from the hotbar when the inventory is not opened

    public GameObject inventorySlotPrefab;
    public GameObject inventoryItemPrefab;

    [HideInInspector] public GameObject playerInventoryObj;
    [HideInInspector] public GameObject shopInventoryObj;
    [HideInInspector] public GameObject pedestalMenuObj;
    [HideInInspector] public GameObject cauldronCraftingObj;

    List<InventorySlot> inventorySlots = new List<InventorySlot>();
    List<InventorySlot> hotbarSlots = new List<InventorySlot>();

    public InventoryItem draggingItem = null;

    public bool showAreas;
    [SerializeField] GameObject dropArea;
    [SerializeField] GameObject trashArea;

    [Header("Item Dropping")]
    public float dropRange = 2f; // Maximum distance a dropped item will drop from the player
    public float dropStrength = 1.5f; // How high the "jump" arc is when dropping an item
    public float dropDuration = 0.6f; // How long it takes for the item to reach it's dropped position

    public GameObject pickupablePrefab;

    GameObject playerObj;
    Player player;

    //at start no slot set "active" nothing stopping play from just clicking a number
    int selectedSlot = -1;

    private void Awake() {
        currentInstance = this;
        InitializeInventorySlots();

        playerInventoryObj = transform.Find("PlayerInventory").gameObject;
        shopInventoryObj = transform.Find("ShopInventory").gameObject;
        pedestalMenuObj = transform.Find("PedestalMenu").gameObject;
        cauldronCraftingObj = transform.Find("PotionCrafting").gameObject;

        playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.GetComponent<Player>();
    }

    public static List<Recipe> GetAllCauldronRecipes() {
        return currentInstance.globalCraftingRecipes.FindAll(x => x.craftingWorkstation == Workstation.Cauldron);
    }

    private void Start() {
        ChangeSelectedSlot(0); // Automatically picks the first slot on start
    }

    //updates active slot when key is pressed
    private void Update()
    {
        //checks for input
        if (Input.inputString != null)
        {
            //checks if input is a number key and switches active slot to key pressed
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if(isNumber && number > 0 && number <= numHotbarSlots)
            {
                ChangeSelectedSlot(number - 1);
            }
        }
    }

    void InitializeInventorySlots() {
        // Initializes inventory slots
        inventorySlots.AddRange(transform.Find("HotBar").GetComponentsInChildren<InventorySlot>()); // Adds all the InventorySlots from the Hotbar to the list
        inventorySlots.AddRange(transform.Find("PlayerInventory").GetComponentsInChildren<InventorySlot>()); // Adds all the InvetorySlots from the PlayerInventory to the list

        // Iniitializes hotbar slots
        hotbarSlots.AddRange(transform.Find("HotBar").GetComponentsInChildren<InventorySlot>()); // Adds all the InventorySlots from the Hotbar to the list
        numHotbarSlots = hotbarSlots.Count;
    }

    public void ToggleInventory(InventoryOpening state, GameObject itemOpening) {
        if (player.itemOpened == itemOpening) state = InventoryOpening.Closing; // If any inventories are open, we want to close inventories

        player.itemOpened = itemOpening; // This gets initialized to true. Closing will set it to false.
        hotbarCover.SetActive(false);
        switch (state) {
            case InventoryOpening.PlayerInventory:
                playerInventoryObj.SetActive(true);
                playerInventoryObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                dropArea.SetActive(true);
                trashArea.SetActive(true);
                break;
            case InventoryOpening.ShopInventory:
                playerInventoryObj.SetActive(true);
                playerInventoryObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -200);

                shopInventoryObj.SetActive(true);
                shopInventoryObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);
                break;
            case InventoryOpening.ItemPedestal:
                playerInventoryObj.SetActive(true);
                playerInventoryObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -200);

                shopInventoryObj.SetActive(true);
                shopInventoryObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 200);

                pedestalMenuObj.SetActive(true);
                break;
            case InventoryOpening.PotionCrafting:
                playerInventoryObj.SetActive(true);
                playerInventoryObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, -200);

                shopInventoryObj.SetActive(true);
                shopInventoryObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-200, 200);

                cauldronCraftingObj.SetActive(true);
                break;
            case InventoryOpening.Closing:
                dropArea.SetActive(false);
                trashArea.SetActive(false);
                playerInventoryObj.SetActive(false);

                shopInventoryObj.SetActive(false);

                foreach(InventorySlot slot in pedestalMenuObj.GetComponentsInChildren<InventorySlot>()) {
                    slot.gameObject.SetActive(false);
                }
                pedestalMenuObj.SetActive(false);

                cauldronCraftingObj.SetActive(false);

                player.itemOpened = null;
                hotbarCover.SetActive(true);
                break;
            default:
                break;
        }
    }

    //code for changing slot
    void ChangeSelectedSlot(int newValue)
    {
        //deactivates old active slot
        if (selectedSlot >= 0)
        {
            hotbarSlots[selectedSlot].Deselect();
        }
        //sets new slot to active
        hotbarSlots[newValue].Select();
        selectedSlot = newValue;
    }

    //adds item to inventory or stack when possible
    public bool AddItem(Item item)
    {
        // Checks if slot has the same item with lower count than max
        foreach (InventorySlot slot in inventorySlots) {
            if (slot.IsEmptySlot()) continue; // Skips it if it was an empty slot

            // Checks if it has InventoryItem and is not a full stack
            InventoryItem inventoryItem = slot.GetItemInSlot();
            if(inventoryItem != null && inventoryItem.item == item && inventoryItem.count < inventoryItem.item.maxStackSize) {
                inventoryItem.count++;
                inventoryItem.RefreshCount();
                return true;
            }
        }

        // Finds empty slot to put item into
        InventorySlot newSlot = inventorySlots.Find(slot => slot.IsEmptySlot()); // Finds the first slot in the list where slot.IsEmptySlot() is true
        if (newSlot != null) { // If one was successfully found
            SpawnNewItem(item, newSlot);
            return true;
        }

        // No available slot was found
        return false;
    }

    public void DropItem(Item item, int count = 1) {
        for(int i =0; i < count; i++)
        {
            GameObject droppedItem = ObjectPoolManager.SpawnObject(pickupablePrefab, playerObj.transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Pickupables);
            Pickupable pickupScript = droppedItem.GetComponent<Pickupable>();
            pickupScript.UpdatePickupableObj(item);
            pickupScript.canPickup = false;

            Vector3 originalSize = droppedItem.transform.localScale;
            droppedItem.transform.localScale = originalSize * 0.5f;

            // Using DOTween package. Jump to a random position within dropRange. After animation, run the pickup's OnItemSpawn script.
            droppedItem.transform.DOJump(playerObj.transform.position + (Vector3)Random.insideUnitCircle * dropRange, dropStrength, 1, dropDuration).onComplete = pickupScript.OnItemSpawn;
            droppedItem.transform.DOScale(originalSize, dropDuration * 0.8f); // Scales the object up smoothly in dropDuration length * 0.8f (when it's 80% done)
        }
    }

    //puts new item into empty slot
    public InventoryItem SpawnNewItem(Item item, InventorySlot slot)
    {
        //new game object made from inventoryItemPrefab
        GameObject newItemGO = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGO.GetComponent<InventoryItem>();
        //sets details about item
        inventoryItem.InitializeItem(item);
        return inventoryItem;
    }

    //if function(true) item is used otherwise just tells what item is in slot
    public Item GetSelectedItem(bool use)
    {
        //gets slot then gets item from slot
        InventoryItem itemInSlot = hotbarSlots[selectedSlot].GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;
            //if wants to be used
            if (use == true)
            {
                //lowers stack count
                itemInSlot.count--;
                //if stack is empty remove the item from inventory
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
                //if stack not empty, refreshes count value
                else
                {
                    itemInSlot.RefreshCount();
                }
            }
            //if not being used returns item in slot
            return itemInSlot.item;
        }
        return null;
    }
}
