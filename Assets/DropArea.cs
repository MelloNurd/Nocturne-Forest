using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] InventoryManager inventoryManager;
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem drugItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        inventoryManager.DropItem(drugItem.item, drugItem.count);
        Destroy(drugItem.gameObject);
    }
}
