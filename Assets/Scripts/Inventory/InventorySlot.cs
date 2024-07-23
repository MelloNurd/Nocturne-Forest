using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;

    public UnityEvent onDropCall;
    public UnityEvent onItemLeave;

    private void Awake()
    {
        Deselect();
    }

    //sets active slot to the active slot color
    public void Select()
    {
        image.color = selectedColor;
    }

    //sets inactive slots to the inactive color
    public void Deselect()
    {
        image.color = notSelectedColor;
    }

    public InventoryItem GetItemInSlot() {
        if (transform.childCount <= 0) return null; // If there are no children, return null
        else return GetComponentInChildren<InventoryItem>(); // If there are children, return any InventoryItem component in children (possible to be null as well)
    }

    public bool IsEmptySlot() {
        return transform.childCount <= 0; // Returns true if there are no children
    }

    //if item's icon is dropped on slot, set as new slot
    public void OnDrop(PointerEventData eventData)
    {
        //if slot is empty then put item into slot
        if (transform.childCount <= 0)
        {
            InventoryItem invItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            invItem.parentAfterDrag = transform;
        }
        //if slot isn't empty then swap the items spots
        else
        {
            InventoryItem droppedItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            InventoryItem existingItem = transform.GetComponentInChildren<InventoryItem>();

            if (droppedItem.item == existingItem.item && existingItem.count < existingItem.item.maxStackSize) {
                if(droppedItem.count + existingItem.count > existingItem.item.maxStackSize) {
                    int difference = existingItem.item.maxStackSize - existingItem.count;
                    existingItem.count += difference;
                    droppedItem.count -= difference;
                }
                else {
                    existingItem.count = existingItem.count + droppedItem.count;
                    Destroy(droppedItem.gameObject);

                }
                existingItem.RefreshCount();
                droppedItem.RefreshCount();
            }
            else {
                Transform temp = transform;
                existingItem.transform.SetParent(droppedItem.parentAfterDrag);
                droppedItem.parentAfterDrag = temp;
            }

        }

        onDropCall?.Invoke();
    }

    public void OnItemLeave() {
        onItemLeave?.Invoke();
    }
}
