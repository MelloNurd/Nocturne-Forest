using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public int maxStackedItems = 4;
    public int numSlots = 7;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

    //at start no slot set "active" nothing stopping play from just clicking a number
    int selectedSlot = -1;
    
    //updates active slot when key is pressed
    private void Update()
    {
        //checks for input
        if (Input.inputString != null)
        {
            //checks if input is a number key and switches active slot to key pressed
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if(isNumber && number > 0 && number <= numSlots)
            {
                ChangeSelectedSlot(number - 1);
            }
        }
    }

    //code for changing slot
    void ChangeSelectedSlot(int newValue)
    {
        //deactivates old active slot
        if (selectedSlot >= 0)
        {
            inventorySlots[selectedSlot].Deselect();
        }
        //sets new slot to active
        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }

    //adds item to inventory or stack when possible
   public bool AddItem(Item item)
    {
        //check if slot has the same item with lower count than max
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackedItems && itemInSlot.item.stackable == true)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }

        //finds empty slot to put item into
        for ( int i = 0; i < inventorySlots.Length; i++)
        {
            //selects slot
            InventorySlot slot = inventorySlots[i];
            //checks if slot is empty
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            //if empty puts item in
            if (itemInSlot == null) 
            { 
                //puts item in new slot
                SpawnNewItem(item, slot);
                return true;
            }
        }
        //returns false if no available slots
        return false;
    }

    //puts new item into empty slot
    void SpawnNewItem(Item item, InventorySlot slot)
    {
        //new game object made from inventoryItemPrefab
        GameObject newItemGO = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGO.GetComponent<InventoryItem>();
        //sets details about item
        inventoryItem.InitializeItem(item);
    }

    //if function(true) item is used otherwise just tells what item is in slot
    public Item GetSelectedItem(bool use)
    {
        //gets slot then gets item from slot
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;
            //if wants to be used
            if (use == true)
            {
                //lowers stack count
                itemInSlot.count--;
                //if stack is empty remove the item from inventory
                if(itemInSlot.count <= 0)
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
