using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script used by buttons to call add/remove items. Probably able to be deleted later

public class DemoScript : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemsToPickup;

    public void PickupItem(int id)
    {
        bool result = inventoryManager.AddItem(itemsToPickup[id]);
        //debugging
        if(result == true)
        {
            Debug.Log("Item added id:" + id);
        }
        else
        {
            Debug.Log("Item NOT added");
        }
    }

    public void GetSelectedItem(bool use)
    {
        Item receivedItem = inventoryManager.GetSelectedItem(false);
        //debugging
        if (receivedItem != null) 
        {
            Debug.Log("Received item: " + receivedItem);
        }
        else
        {
            Debug.Log("No item received");
        }
    }

    public void UseSelectedItem(bool use)
    {
        Item receivedItem = inventoryManager.GetSelectedItem(true);
        //debugging
        if (receivedItem != null)
        {
            Debug.Log("Used item: " + receivedItem);
        }
        else
        {
            Debug.Log("No item used");
        }
    }
}
