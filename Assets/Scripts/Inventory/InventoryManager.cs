using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public enum InventoryOpening {
        PlayerInventory,
        ShopInventory,
        ItemPedestal,
        PotionCrafting,
        ShopBook,
        DoorMenu,
        Closing
    }

    public static InventoryManager currentInstance; // There should only ever be one InventoryManager per scene, so we are doing this for easy access.

    public List<Recipe> globalCraftingRecipes = new List<Recipe>();
    public List<Item> globalItemList = new List<Item>();

    public static Dictionary<string, Item> itemLookup = new Dictionary<string, Item>(); // A dictionary where you can get an Item object from the item's name

    public int playerCash;
    
    int numHotbarSlots;
    [SerializeField] GameObject hotbarCover; // This is just to block mouse dragging from the hotbar when the inventory is not opened

    public GameObject inventorySlotPrefab;
    public GameObject inventoryItemPrefab;
    public GameObject itemTitleObj;
    public TMP_Text playerCashText;
    public TMP_Text openCloseShopText;

    Tween titleTween;

    [HideInInspector] public GameObject playerInventoryObj;
    [HideInInspector] public GameObject hotbarObj;
    [HideInInspector] public GameObject shopInventoryObj;
    [HideInInspector] public GameObject pedestalMenuObj;
    [HideInInspector] public GameObject cauldronCraftingObj;
    [HideInInspector] public GameObject shopBookObj;
    [HideInInspector] public GameObject doorMenuObj;

    List<InventorySlot> shopInventorySlots = new List<InventorySlot>();
    List<InventorySlot> playerInventorySlots = new List<InventorySlot>();
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

        playerInventoryObj = transform.Find("PlayerInventory").gameObject;
        hotbarObj = transform.Find("HotBar").gameObject;
        shopInventoryObj = transform.Find("ShopInventory").gameObject;
        pedestalMenuObj = transform.Find("PedestalMenu").gameObject;
        cauldronCraftingObj = transform.Find("PotionCrafting").gameObject;
        shopBookObj = transform.Find("ShopBookMenu").gameObject;
        doorMenuObj = transform.Find("DoorMenu").gameObject;

        playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.GetComponent<Player>();

        if (itemLookup.Count <= 0) {
            foreach (Item item in globalItemList) { // Initialize itemLookup dictionary
                itemLookup.Add(item.name, item);
            }
        }
    }

    public static List<Recipe> GetAllCauldronRecipes() {
        return currentInstance.globalCraftingRecipes.FindAll(x => x.craftingWorkstation == Workstation.Cauldron);
    }

    private void Start() {
        InitializeInventorySlots();
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

    private void OnDisable() {
        SaveInventory();
    }

    private void OnEnable() {
        LoadInventory();
    }

    public void HideItemTitle() {
        titleTween = itemTitleObj.GetComponent<TMP_Text>().DOColor(Color.clear, 0.5f).SetDelay(0.8f);
    }
    public void KillHideItemTitle() {
        itemTitleObj.GetComponent<TMP_Text>().color = Color.white;
        titleTween.Kill();
    }

    void SaveInventory() {
        // Player's Money
        PlayerPrefs.SetInt("player-money", playerCash);

        // Basically, we're gonna try to do it by every single slot. Loop through them, load data based on the slot name.
        // If this is too slow, we're gonna have to basically index every slot that is not empty and save the item plus its slot into the saved string.

        // Shop's Chest Inventory
        foreach (InventorySlot slot in shopInventorySlots) {
            InventoryItem item = slot.GetItemInSlot();
            if (item == null) PlayerPrefs.DeleteKey("Shop" + slot.gameObject.name); // If no item in slot, delete anything saved for slot
            else PlayerPrefs.SetString("Shop" + slot.gameObject.name, item.item.name + ";" + item.count); // Otherwise, save what is in slot
        }

        // Players's Inventory
        foreach (InventorySlot slot in playerInventorySlots) {
            InventoryItem item = slot.GetItemInSlot();
            if (item == null) PlayerPrefs.DeleteKey("Player" + slot.gameObject.name);
            else PlayerPrefs.SetString("Player" + slot.gameObject.name, item.item.name + ";" + item.count);
        }

        // Players's Hotbar
        foreach (InventorySlot slot in hotbarSlots) {
            InventoryItem item = slot.GetItemInSlot();
            if (item == null) PlayerPrefs.DeleteKey(slot.gameObject.name);
            else PlayerPrefs.SetString(slot.gameObject.name, item.item.name + ";" + item.count);
        }
    }

    void LoadInventory() {
        // Player's Money
        int money = PlayerPrefs.GetInt("player-money", -1);
        if (money != -1) playerCash = money;
        else Debug.LogWarning("Unable to load player's money!");
        playerCashText.text = '$' + playerCash.ToString();

        // Shop's Chest Inventory
        foreach (InventorySlot slot in shopInventorySlots) {
            string data = PlayerPrefs.GetString("Shop" + slot.gameObject.name, "");
            if (string.IsNullOrEmpty(data)) continue;

            string itemToSpawn = data.Split(';')[0];
            int amount = int.Parse(data.Split(';')[1]);

            if (itemLookup.TryGetValue(itemToSpawn, out Item _item)) {
                InventoryItem newItem = SpawnNewItem(_item, slot);
                newItem.SetCount(amount);
            }
        }

        // Players's Inventory
        foreach (InventorySlot slot in playerInventorySlots) {
            string data = PlayerPrefs.GetString("Player" + slot.gameObject.name, "");
            if (string.IsNullOrEmpty(data)) continue;

            string itemToSpawn = data.Split(';')[0];
            int amount = int.Parse(data.Split(';')[1]);

            if (itemLookup.TryGetValue(itemToSpawn, out Item _item)) {
                InventoryItem newItem = SpawnNewItem(_item, slot);
                newItem.SetCount(amount);
            }
        }

        // Players's Hotbar
        foreach (InventorySlot slot in hotbarSlots) {
            string data = PlayerPrefs.GetString(slot.gameObject.name, "");
            if (string.IsNullOrEmpty(data)) continue;

            string itemToSpawn = data.Split(';')[0];
            int amount = int.Parse(data.Split(';')[1]);

            if (itemLookup.TryGetValue(itemToSpawn, out Item _item)) {
                InventoryItem newItem = SpawnNewItem(_item, slot);
                newItem.SetCount(amount);
            }
        }
    }

    void InitializeInventorySlots() {
        shopInventorySlots.AddRange(transform.Find("ShopInventory").GetComponentsInChildren<InventorySlot>()); // Adds all the InventorySlots from the Hotbar to the list
        
        // Initializes inventory slots
        playerInventorySlots.AddRange(transform.Find("PlayerInventory").GetComponentsInChildren<InventorySlot>()); // Adds all the InvetorySlots from the PlayerInventory to the list

        // Iniitializes hotbar slots
        hotbarSlots.AddRange(transform.Find("HotBar").GetComponentsInChildren<InventorySlot>()); // Adds all the InventorySlots from the Hotbar to the list
        numHotbarSlots = hotbarSlots.Count;

        LoadInventory();
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
            case InventoryOpening.ShopBook:
                hotbarObj.SetActive(false);

                shopBookObj.SetActive(true);
                break;
            case InventoryOpening.DoorMenu:
                doorMenuObj.SetActive(true);
                break;
            case InventoryOpening.Closing:
                hotbarObj.SetActive(true);

                dropArea.SetActive(false);
                trashArea.SetActive(false);
                playerInventoryObj.SetActive(false);

                shopInventoryObj.SetActive(false);

                foreach(InventorySlot slot in pedestalMenuObj.GetComponentsInChildren<InventorySlot>()) {
                    slot.gameObject.SetActive(false);
                }
                pedestalMenuObj.SetActive(false);

                cauldronCraftingObj.SetActive(false);

                shopBookObj.SetActive(false);

                doorMenuObj.SetActive(false);

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
        // Check hotbar slots for a slot that contains the pickup item
        foreach (InventorySlot slot in hotbarSlots) {
            if (slot.IsEmptySlot()) continue; // Skips it if it was an empty slot

            // Checks if it has InventoryItem and is not a full stack
            InventoryItem inventoryItem = slot.GetItemInSlot();
            if (inventoryItem != null && inventoryItem.item == item && inventoryItem.count < inventoryItem.item.maxStackSize) {
                inventoryItem.count++;
                inventoryItem.RefreshCount();
                return true;
            }
        }

        // If no hotbar slots found, do the same for the playerInventory slots
        foreach (InventorySlot slot in playerInventorySlots) {
            if (slot.IsEmptySlot()) continue; // Skips it if it was an empty slot

            // Checks if it has InventoryItem and is not a full stack
            InventoryItem inventoryItem = slot.GetItemInSlot();
            if (inventoryItem != null && inventoryItem.item == item && inventoryItem.count < inventoryItem.item.maxStackSize) {
                inventoryItem.count++;
                inventoryItem.RefreshCount();
                return true;
            }
        }

        // No existing slots with item found in inventory

        // Finds empty slot to put item into
        InventorySlot newSlot = hotbarSlots.Find(slot => slot.IsEmptySlot()); // Finds the first slot in the list where slot.IsEmptySlot() is true
        if (newSlot != null) { // If one was successfully found
            SpawnNewItem(item, newSlot);
            return true;
        }

        newSlot = playerInventorySlots.Find(slot => slot.IsEmptySlot()); // Finds the first slot in the list where slot.IsEmptySlot() is true
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
            pickupScript.playerDropped = true;

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
