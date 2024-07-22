using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class DropArea : MonoBehaviour, IDropHandler
{
    InventoryManager inventoryManager;
    public bool trash;
    [SerializeField] Button button;

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

