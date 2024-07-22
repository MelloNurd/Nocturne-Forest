using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    InventoryManager inventoryManager;
    public bool trash;

    private void Start() {
        inventoryManager = InventoryManager.currentInstance;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem drugItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if(!trash)
        {
            inventoryManager.DropItem(drugItem.item, drugItem.count);
        }
        Destroy(drugItem.gameObject);
    }
}
