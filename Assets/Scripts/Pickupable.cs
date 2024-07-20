using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    public Item item;

    public bool canPickup = true;

    public int stackSize = 1; // not yet implemented

    GameObject player;
    float playerDist;
    float playerPickupRange;

    InventoryManager playerInventory;

    bool isInRange;

    GameObject shadow;


    private void Awake() {
        if(item == null) {
            Debug.LogError("Interactable with no item assigned found: " + gameObject.name);
            canPickup = false;
        }

        player = GameObject.Find("Player");
        playerPickupRange = player.GetComponent<Player>().interactionRange;

        playerInventory = player.GetComponent<InventoryManager>();

        shadow = transform.Find("Shadow").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        playerDist = Vector2.Distance(transform.position, player.transform.position);
        isInRange = playerDist <= playerPickupRange;
        shadow.SetActive(isInRange && canPickup);
    }

    public void Pickup() {
        if (!canPickup || !isInRange) return;

        playerInventory.AddItem(item);
        ObjectPoolManager.ReturnObjectToPool(gameObject);
    }

    public void UpdatePickupableObj(Item newItem) {
        GetComponentInChildren<SpriteRenderer>().sprite = newItem.image;
        item = newItem;
    }

    public void OnItemSpawn() { // This is called at the end of the "jump" animation when an item drops
        canPickup = true;
        // play a sound and animation or something
    }
}
