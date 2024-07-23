using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image image;
    public TMP_Text countText;

    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

    Player player;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    //set's item's details
    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
    }

    //changes the count number on item stack
    public void RefreshCount()
    {
        //changes count value to text
        countText.text = count.ToString();
        //if more than one item in stack displays text
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }

    //picks up item's icon and sets an origin for if dropped in invalid spot
    public void OnBeginDrag(PointerEventData eventData)
    {
        InventorySlot slot = transform.parent.GetComponent<InventorySlot>();
        if(slot != null) {
            slot.OnItemLeave();
        }

        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);

        InventoryManager.currentInstance.draggingItem = this;
    }

    //moves item's icon when being dragged in inventory
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition + Vector3.forward;
        /*if (Input.GetMouseButtonDown(1)) // Right-click
        {
            Debug.Log("Right-click detected during drag");
            DropOneItem();
        }*/
    }

    //drops item's icon and either snaps into a slot or goes back to original slot
    public void OnEndDrag(PointerEventData eventData)
    {
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        //if (hit.collider != null)
        //    Debug.Log(hit.collider.gameObject.name);
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        InventoryManager.currentInstance.draggingItem = null;
    }

   /* private void DropOneItem()
    {
        if (count > 1)
        {
            count--;
            RefreshCount();

            // Create a new InventoryItem for the single item
            GameObject newItemObject = Instantiate(gameObject, parentAfterDrag);
            InventoryItem newItem = newItemObject.GetComponent<InventoryItem>();
            newItem.InitializeItem(item);
            newItem.count = 1;
            newItem.RefreshCount();

            // Snap the new item to the nearest valid slot
            image.raycastTarget = true;
            newItem.transform.SetParent(parentAfterDrag);
        }
        else
        {
            image.raycastTarget = true;
            transform.SetParent(parentAfterDrag);
        }
    }*/
}
