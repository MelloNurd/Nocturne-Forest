using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;

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

    //if item's icon is dropped on slot, set as new slot
    public void OnDrop(PointerEventData eventData)
    {
        //if slot is empty then put item into slot
        if (transform.childCount == 0)
        {
            InventoryItem invItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            invItem.parentAfterDrag = transform;
        }
        //if slot isn't empty then swap the items spots
        if(transform.childCount == 1) 
        {
            InventoryItem drugItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            InventoryItem currentItem = transform.GetComponentInChildren<InventoryItem>();
            Transform temp = transform;
            currentItem.transform.SetParent(drugItem.parentAfterDrag);
            drugItem.parentAfterDrag = temp;
        }
    }
}
