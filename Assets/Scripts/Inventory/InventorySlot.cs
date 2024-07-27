using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;

    //public InventoryItem itemInSlot;

    public UnityEvent onDropCall;
    public UnityEvent onItemLeave;
    public UnityEvent onDisableCall;

    PointerEventData eventData;
    List<RaycastResult> raysastResults = new List<RaycastResult>();

    private void Awake()
    {
        Deselect();
        eventData = new PointerEventData(EventSystem.current);
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
        if(IsEmptySlot()) return null;
        else return transform.GetComponentInChildren<InventoryItem>();
    }

    public bool IsEmptySlot() {
        return transform.childCount <= 0;    
    }

    private void Update() {
        // Checks if right click is pressed AND we're dragging an item already (draggingItem != null) AND the mouse is over this slot AND the draggingItem's original slot was not this slot
        if(Input.GetMouseButtonDown(1) && InventoryManager.currentInstance.draggingItem && IsMouseOver() && InventoryManager.currentInstance.draggingItem.slot != this) {
            if(IsEmptySlot()) { // If no items are in this slot
                InventoryItem newItem = InventoryManager.currentInstance.SpawnNewItem(InventoryManager.currentInstance.draggingItem.item, this);
                InventoryManager.currentInstance.draggingItem.count -= 1;
                onDropCall?.Invoke(); // This might break stuff idk
                if(newItem) newItem.RefreshCount();
            }
            else { // If there are items in this slot
                InventoryItem existingItem = transform.GetComponentInChildren<InventoryItem>();
                if (existingItem.count + 1 <= existingItem.item.maxStackSize) {
                    existingItem.count += 1;
                    InventoryManager.currentInstance.draggingItem.count -= 1;
                    onDropCall?.Invoke(); // This might break stuff idk
                }
                existingItem.RefreshCount();
            }

            if (InventoryManager.currentInstance.draggingItem.count <= 0) {
                if (InventoryManager.currentInstance.draggingItem.slot != null) InventoryManager.currentInstance.draggingItem.slot.GetComponent<Image>().color = Color.white;
                Destroy(InventoryManager.currentInstance.draggingItem.gameObject);
            }
            if (InventoryManager.currentInstance.draggingItem) InventoryManager.currentInstance.draggingItem.RefreshCount();
        }
    }

    private bool IsMouseOver() {
        eventData.position = Input.mousePosition;
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults.Any(x => x.gameObject == gameObject);
    }

    //if item's icon is dropped on slot, set as new slot
    public void OnDrop(PointerEventData eventData)
    {
        //if slot is empty then put item into slot
        if (IsEmptySlot())
        {
            InventoryItem invItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            invItem.parentAfterDrag = transform;
        }
        //if slot isn't empty then swap the items spots
        else
        {
            InventoryItem droppedItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            InventoryItem existingItem = transform.GetComponentInChildren<InventoryItem>();

            if (droppedItem.item == existingItem.item) { // If the items are the same, we want to try merging the stacks
                if (existingItem.count < existingItem.item.maxStackSize) { // If the existing item slot is not full
                    if (droppedItem.count + existingItem.count > existingItem.item.maxStackSize) { // If we have more items than the maxStackSize
                        int difference = existingItem.item.maxStackSize - existingItem.count;
                        existingItem.count += difference;
                        droppedItem.count -= difference;
                    }
                    else { // If we don't have more items than maxStackSize, simply increase count of existing slot
                        existingItem.count = existingItem.count + droppedItem.count;
                        if (InventoryManager.currentInstance.draggingItem.slot != null) InventoryManager.currentInstance.draggingItem.slot.GetComponent<Image>().color = Color.white;
                        Destroy(droppedItem.gameObject);
                    }
                    existingItem.RefreshCount();
                    droppedItem.RefreshCount();
                }
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

    private void OnDisable() {
        onDisableCall?.Invoke();
    }
}
